using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using InventoryAPI.Interfaces;
using InventoryAPI.Models;

namespace InventoryAPI.Services
{
    public class RabbitMQPublisher : IRabbitMQPublisher, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQPublisher> _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly object _lock = new object();
        private bool _disposed = false;

        // Circuit Breaker simple
        private int _failureCount = 0;
        private DateTime _lastFailureTime = DateTime.MinValue;
        private readonly int _circuitBreakerThreshold = 5;
        private readonly TimeSpan _circuitBreakerTimeout = TimeSpan.FromMinutes(1);

        public RabbitMQPublisher(IOptions<RabbitMQSettings> settings, ILogger<RabbitMQPublisher> logger)
        {
            _settings = settings.Value;
            _logger = logger;
        }

        private void InitializeConnection()
        {
            try
            {
                // Circuit Breaker check
                if (IsCircuitBreakerOpen())
                {
                    throw new InvalidOperationException("Circuit breaker is open. RabbitMQ is temporarily unavailable.");
                }

                var factory = new ConnectionFactory()
                {
                    HostName = _settings.HostName,
                    Port = _settings.Port,
                    UserName = _settings.UserName,
                    Password = _settings.Password,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                    RequestedHeartbeat = TimeSpan.FromSeconds(60),
                    RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                // Declarar el exchange
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                // Declarar las colas para diferentes tipos de eventos
                DeclareQueues();

                // Reset circuit breaker on successful connection
                _failureCount = 0;

                _logger.LogInformation("RabbitMQ connection established successfully");
            }
            catch (Exception ex)
            {
                RecordFailure();
                _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
                throw;
            }
        }

        private void DeclareQueues()
        {
            if (_channel == null) return;

            var queues = new[]
            {
                "product.created",
                "product.updated",
                "product.deleted"
            };

            foreach (var queueName in queues)
            {
                _channel.QueueDeclare(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _channel.QueueBind(
                    queue: queueName,
                    exchange: _settings.ExchangeName,
                    routingKey: queueName);

                _logger.LogDebug("Queue {QueueName} declared and bound", queueName);
            }
        }

        public async Task PublishAsync<T>(string routingKey, T message)
        {
            const int maxRetries = 3;
            int retryCount = 0;

            while (retryCount < maxRetries)
            {
                try
                {
                    await PublishMessageInternal(routingKey, message);
                    return; // Success, exit retry loop
                }
                catch (Exception ex)
                {
                    retryCount++;
                    _logger.LogWarning(ex, "Retry {RetryCount}/{MaxRetries} for publishing message to {RoutingKey}",
                        retryCount, maxRetries, routingKey);

                    if (retryCount >= maxRetries)
                    {
                        RecordFailure();
                        _logger.LogError(ex, "Failed to publish message after {MaxRetries} retries", maxRetries);
                        throw;
                    }

                    // Exponential backoff
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount));
                    await Task.Delay(delay);

                    // Try to reinitialize connection
                    try
                    {
                        lock (_lock)
                        {
                            if (_channel?.IsClosed != false)
                            {
                                InitializeConnection();
                            }
                        }
                    }
                    catch (Exception initEx)
                    {
                        _logger.LogWarning(initEx, "Failed to reinitialize connection on retry {RetryCount}", retryCount);
                    }
                }
            }
        }

        private async Task PublishMessageInternal<T>(string routingKey, T message)
        {
            await Task.Run(() =>
            {
                lock (_lock)
                {
                    if (_disposed)
                    {
                        throw new ObjectDisposedException(nameof(RabbitMQPublisher));
                    }

                    if (_channel == null || _channel.IsClosed)
                    {
                        InitializeConnection();
                    }

                    var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    var body = Encoding.UTF8.GetBytes(json);

                    var properties = _channel!.CreateBasicProperties();
                    properties.Persistent = true;
                    properties.MessageId = Guid.NewGuid().ToString();
                    properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
                    properties.DeliveryMode = 2; // Persistent

                    _channel.BasicPublish(
                        exchange: _settings.ExchangeName,
                        routingKey: routingKey,
                        basicProperties: properties,
                        body: body);

                    _logger.LogInformation("Message published successfully to {RoutingKey} with ID {MessageId}",
                        routingKey, properties.MessageId);
                }
            });
        }

        public async Task PublishProductEventAsync(string action, Product product)
        {
            var routingKey = action.ToLower() switch
            {
                "create" => "product.created",
                "update" => "product.updated",
                "delete" => "product.deleted",
                _ => throw new ArgumentException($"Invalid action: {action}")
            };

            var eventMessage = new
            {
                Action = action,
                ProductId = product.Id,
                Product = product,
                Timestamp = DateTime.UtcNow
            };

            await PublishAsync(routingKey, eventMessage);
        }

        private bool IsCircuitBreakerOpen()
        {
            if (_failureCount >= _circuitBreakerThreshold)
            {
                var timeSinceLastFailure = DateTime.UtcNow - _lastFailureTime;
                if (timeSinceLastFailure < _circuitBreakerTimeout)
                {
                    _logger.LogWarning("Circuit breaker is open. Time until reset: {TimeRemaining}",
                        _circuitBreakerTimeout - timeSinceLastFailure);
                    return true;
                }
                else
                {
                    // Reset circuit breaker
                    _failureCount = 0;
                    _logger.LogInformation("Circuit breaker reset after timeout");
                }
            }
            return false;
        }

        private void RecordFailure()
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;

            if (_failureCount >= _circuitBreakerThreshold)
            {
                _logger.LogError("Circuit breaker opened after {FailureCount} failures", _failureCount);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                lock (_lock)
                {
                    _disposed = true;
                    _channel?.Close();
                    _channel?.Dispose();
                    _connection?.Close();
                    _connection?.Dispose();
                }
                _logger.LogInformation("RabbitMQ Publisher disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ Publisher");
            }
        }
    }
}