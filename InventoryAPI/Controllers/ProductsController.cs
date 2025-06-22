using Microsoft.AspNetCore.Mvc;
using InventoryAPI.DTOs;
using InventoryAPI.Interfaces;

namespace InventoryAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los productos
        /// </summary>
        /// <returns>Lista de productos</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                _logger.LogInformation("Getting all products");
                var products = await _productService.GetAllAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting products");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener un producto por ID
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Producto encontrado</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                _logger.LogInformation("Getting product with ID {ProductId}", id);
                var product = await _productService.GetByIdAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found", id);
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Crear un nuevo producto
        /// </summary>
        /// <param name="createProductDto">Datos del producto a crear</param>
        /// <returns>Producto creado</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto createProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating new product: {ProductName}", createProductDto.Name);
                var product = await _productService.CreateAsync(createProductDto);

                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = product.Id },
                    product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating product");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Actualizar un producto existente
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <param name="updateProductDto">Datos actualizados del producto</param>
        /// <returns>Producto actualizado</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ProductDto>> UpdateProduct(int id, [FromBody] UpdateProductDto updateProductDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Updating product with ID {ProductId}", id);
                var product = await _productService.UpdateAsync(id, updateProductDto);

                if (product == null)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for update", id);
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating product with ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Eliminar un producto
        /// </summary>
        /// <param name="id">ID del producto</param>
        /// <returns>Confirmación de eliminación</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            try
            {
                _logger.LogInformation("Deleting product with ID {ProductId}", id);
                var deleted = await _productService.DeleteAsync(id);

                if (!deleted)
                {
                    _logger.LogWarning("Product with ID {ProductId} not found for deletion", id);
                    return NotFound($"Producto con ID {id} no encontrado");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product with ID {ProductId}", id);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Endpoint de salud para verificar el estado del servicio
        /// </summary>
        /// <returns>Estado del servicio</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult GetHealth()
        {
            return Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });
        }
    }
}