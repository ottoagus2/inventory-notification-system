using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Services
{
    public class InventoryLogService : IInventoryLogService
    {
        private readonly NotificationDbContext _context;
        private readonly ILogger<InventoryLogService> _logger;

        public InventoryLogService(NotificationDbContext context, ILogger<InventoryLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<InventoryLog> CreateLogAsync(int productId, string action, string productData)
        {
            try
            {
                var log = new InventoryLog
                {
                    ProductId = productId,
                    Action = action,
                    ProductData = productData,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "NotificationService",
                    IsSuccessful = true
                };

                _context.InventoryLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Inventory log created for Product {ProductId} with action {Action}",
                    productId, action);

                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating inventory log for Product {ProductId}", productId);

                // Crear log de error
                var errorLog = new InventoryLog
                {
                    ProductId = productId,
                    Action = action,
                    ProductData = productData,
                    ProcessedAt = DateTime.UtcNow,
                    ProcessedBy = "NotificationService",
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };

                try
                {
                    _context.InventoryLogs.Add(errorLog);
                    await _context.SaveChangesAsync();
                }
                catch (Exception logEx)
                {
                    _logger.LogError(logEx, "Failed to save error log");
                }

                throw;
            }
        }

        public async Task<IEnumerable<InventoryLog>> GetLogsAsync()
        {
            try
            {
                return await _context.InventoryLogs
                    .OrderByDescending(l => l.ProcessedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory logs");
                throw;
            }
        }

        public async Task<IEnumerable<InventoryLog>> GetLogsByProductIdAsync(int productId)
        {
            try
            {
                return await _context.InventoryLogs
                    .Where(l => l.ProductId == productId)
                    .OrderByDescending(l => l.ProcessedAt)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory logs for Product {ProductId}", productId);
                throw;
            }
        }
    }
}