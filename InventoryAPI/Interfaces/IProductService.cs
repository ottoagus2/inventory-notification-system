using InventoryAPI.DTOs;
using InventoryAPI.Models;

namespace InventoryAPI.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllAsync();
        Task<ProductDto?> GetByIdAsync(int id);
        Task<ProductDto> CreateAsync(CreateProductDto createProductDto);
        Task<ProductDto?> UpdateAsync(int id, UpdateProductDto updateProductDto);
        Task<bool> DeleteAsync(int id);
    }

    public interface IRabbitMQPublisher
    {
        Task PublishAsync<T>(string routingKey, T message);
        Task PublishProductEventAsync(string action, Product product);
    }
}


