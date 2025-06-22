using NotificationService.Models;

namespace NotificationService.Interfaces
{
    public interface IInventoryLogService
    {
        Task<InventoryLog> CreateLogAsync(int productId, string action, string productData);
        Task<IEnumerable<InventoryLog>> GetLogsAsync();
        Task<IEnumerable<InventoryLog>> GetLogsByProductIdAsync(int productId);
    }

    public interface IRabbitMQConsumer
    {
        Task StartConsumingAsync(CancellationToken cancellationToken);
        Task StopConsumingAsync();
    }
}