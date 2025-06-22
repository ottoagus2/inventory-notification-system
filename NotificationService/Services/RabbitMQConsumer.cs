using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class RabbitMQConsumer : IRabbitMQConsumer, IDisposable
    {
        private readonly RabbitMQSettings _settings;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private readonly List<string> _consumerTags = new();
        private bool _disposed = false;

        public RabbitMQConsumer(
            IOptions<RabbitMQSettings> settings,
            ILogger<RabbitMQConsumer> logger,
            IServiceProvider serviceProvider)
        {
            _settings = settings.Value;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public async Task StartConsumingAsync(CancellationToken cancellationToken)
        {
            try
            {
                InitializeConnection();
                await SetupConsumers();
                _logger.LogInformation("RabbitMQ consumers started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start RabbitMQ consumers");
                throw;
            }
        }

        private void InitializeConnection()
        {
            try
            {
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

                // Configurar QoS para procesar un mensaje a la vez
                _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

                // Asegurar que el exchange y las colas existen
                _channel.ExchangeDeclare(
                    exchange: _settings.ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false);

                DeclareQueues();

                _logger.LogInformation("RabbitMQ consumer connection established");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize RabbitMQ consumer connection");
                throw;
            }
        }

        private void DeclareQueues()
        {
            if (_channel == null) return;

            var queues = new[] { "product.created", "product.updated", "product.deleted" };

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

                _logger.LogDebug("Queue {QueueName} declared and bound for consumer", queueName);
            }
        }

        private async Task SetupConsumers()
        {
            if (_channel == null) return;

            var queues = new[] { "product.created", "product.updated", "product.deleted" };

            foreach (var queueName in queues)
            {
                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    await ProcessMessage(queueName, ea);
                };

                var consumerTag = _channel.BasicConsume(
                    queue: queueName,
                    autoAck: false, // Manual acknowledgment
                    consumer: consumer);

                _consumerTags.Add(consumerTag);
                _logger.LogInformation("Consumer started for queue {QueueName} with tag {ConsumerTag}", queueName, consumerTag);
            }

            await Task.CompletedTask;
        }

        private async Task ProcessMessage(string queueName, BasicDeliverEventArgs ea)
        {
            var messageId = ea.BasicProperties?.MessageId ?? "unknown";

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                _logger.LogInformation("Processing message from {QueueName}: {MessageId}", queueName, messageId);

                // Deserializar el mensaje
                var eventMessage = JsonSerializer.Deserialize<JsonElement>(message);

                var action = eventMessage.GetProperty("action").GetString() ?? "";
                var productId = eventMessage.GetProperty("productId").GetInt32();
                var productJson = eventMessage.GetProperty("product").GetRawText();

                // Procesar el mensaje usando el servicio de logs
                using var scope = _serviceProvider.CreateScope();
                var logService = scope.ServiceProvider.GetRequiredService<IInventoryLogService>();

                await logService.CreateLogAsync(productId, action.ToUpper(), productJson);

                // Acknowledge el mensaje solo si todo fue exitoso
                _channel?.BasicAck(ea.DeliveryTag, false);

                _logger.LogInformation("Message processed successfully for Product {ProductId} with action {Action}",
                    productId, action);
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Invalid JSON format in message {MessageId} from {QueueName}", messageId, queueName);

                // Reject mensaje con formato inválido (no reencolar)
                _channel?.BasicReject(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message {MessageId} from {QueueName}", messageId, queueName);

                try
                {
                    // Intentar crear log de error
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    using var scope = _serviceProvider.CreateScope();
                    var logService = scope.ServiceProvider.GetRequiredService<IInventoryLogService>();

                    var errorLog = new InventoryLog
                    {
                        ProductId = 0, // ID desconocido por el error
                        Action = "ERROR",
                        ProductData = message,
                        ProcessedAt = DateTime.UtcNow,
                        ProcessedBy = "NotificationService",
                        IsSuccessful = false,
                        ErrorMessage = ex.Message
                    };

                    await logService.CreateLogAsync(0, "ERROR", message);
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to log error message {MessageId}", messageId);
                }

                // Rechazar mensaje y reencolar para reintento
                _channel?.BasicReject(ea.DeliveryTag, true);
            }
        }

        public async Task StopConsumingAsync()
        {
            try
            {
                foreach (var consumerTag in _consumerTags)
                {
                    _channel?.BasicCancel(consumerTag);
                    _logger.LogDebug("Consumer cancelled: {ConsumerTag}", consumerTag);
                }
                _consumerTags.Clear();

                _logger.LogInformation("RabbitMQ consumers stopped");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping RabbitMQ consumers");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            try
            {
                _disposed = true;
                StopConsumingAsync().Wait(TimeSpan.FromSeconds(5));

                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();

                _logger.LogInformation("RabbitMQ consumer disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing RabbitMQ consumer connection");
            }
        }
    }
}