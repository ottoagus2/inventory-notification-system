using Microsoft.EntityFrameworkCore;
using InventoryAPI.Data;
using InventoryAPI.DTOs;
using InventoryAPI.Interfaces;
using InventoryAPI.Models;

namespace InventoryAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly InventoryDbContext _context;
        private readonly IRabbitMQPublisher _publisher;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            InventoryDbContext context,
            IRabbitMQPublisher publisher,
            ILogger<ProductService> logger)
        {
            _context = context;
            _publisher = publisher;
            _logger = logger;
        }

        public async Task<IEnumerable<ProductDto>> GetAllAsync()
        {
            try
            {
                var products = await _context.Products.ToListAsync();
                return products.Select(MapToDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all products");
                throw;
            }
        }

        public async Task<ProductDto?> GetByIdAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                return product != null ? MapToDto(product) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<ProductDto> CreateAsync(CreateProductDto createProductDto)
        {
            try
            {
                var product = new Product
                {
                    Name = createProductDto.Name,
                    Description = createProductDto.Description,
                    Price = createProductDto.Price,
                    Stock = createProductDto.Stock,
                    Category = createProductDto.Category,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                // Publicar evento de creación
                await _publisher.PublishProductEventAsync("CREATE", product);

                _logger.LogInformation("Product created with ID {ProductId}", product.Id);

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                throw;
            }
        }

        public async Task<ProductDto?> UpdateAsync(int id, UpdateProductDto updateProductDto)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return null;
                }

                product.Name = updateProductDto.Name;
                product.Description = updateProductDto.Description;
                product.Price = updateProductDto.Price;
                product.Stock = updateProductDto.Stock;
                product.Category = updateProductDto.Category;
                product.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Publicar evento de actualización
                await _publisher.PublishProductEventAsync("UPDATE", product);

                _logger.LogInformation("Product updated with ID {ProductId}", product.Id);

                return MapToDto(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var product = await _context.Products.FindAsync(id);
                if (product == null)
                {
                    return false;
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Publicar evento de eliminación
                await _publisher.PublishProductEventAsync("DELETE", product);

                _logger.LogInformation("Product deleted with ID {ProductId}", id);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                throw;
            }
        }

        private static ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Stock = product.Stock,
                Category = product.Category,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}