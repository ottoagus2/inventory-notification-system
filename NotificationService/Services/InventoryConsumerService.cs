using NotificationService.Interfaces;

namespace NotificationService.Services
{
    public class InventoryConsumerService : BackgroundService
    {
        private readonly IRabbitMQConsumer _consumer;
        private readonly ILogger<InventoryConsumerService> _logger;

        public InventoryConsumerService(IRabbitMQConsumer consumer, ILogger<InventoryConsumerService> logger)
        {
            _consumer = consumer;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Inventory Consumer Service started");

            try
            {
                await _consumer.StartConsumingAsync(stoppingToken);

                // Mantener el servicio corriendo hasta que se cancele
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Inventory Consumer Service is stopping due to cancellation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Inventory Consumer Service");
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Inventory Consumer Service is stopping");

            await _consumer.StopConsumingAsync();
            await base.StopAsync(cancellationToken);

            _logger.LogInformation("Inventory Consumer Service stopped");
        }
    }
}