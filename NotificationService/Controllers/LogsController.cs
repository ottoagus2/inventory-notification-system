using Microsoft.AspNetCore.Mvc;
using NotificationService.Interfaces;
using NotificationService.Models;

namespace NotificationService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LogsController : ControllerBase
    {
        private readonly IInventoryLogService _logService;
        private readonly ILogger<LogsController> _logger;

        public LogsController(IInventoryLogService logService, ILogger<LogsController> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los logs de inventario
        /// </summary>
        /// <returns>Lista de logs</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryLog>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<InventoryLog>>> GetLogs()
        {
            try
            {
                _logger.LogInformation("Getting all inventory logs");
                var logs = await _logService.GetLogsAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory logs");
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener logs por ID de producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Logs del producto específico</returns>
        [HttpGet("product/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<InventoryLog>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<InventoryLog>>> GetLogsByProductId(int productId)
        {
            try
            {
                _logger.LogInformation("Getting inventory logs for product {ProductId}", productId);
                var logs = await _logService.GetLogsByProductIdAsync(productId);
                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting inventory logs for product {ProductId}", productId);
                return StatusCode(500, "Error interno del servidor");
            }
        }

        /// <summary>
        /// Obtener estadísticas de los logs
        /// </summary>
        /// <returns>Estadísticas resumidas</returns>
        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetLogStats()
        {
            try
            {
                _logger.LogInformation("Getting log statistics");
                var logs = await _logService.GetLogsAsync();

                var stats = new
                {
                    TotalLogs = logs.Count(),
                    SuccessfulLogs = logs.Count(l => l.IsSuccessful),
                    FailedLogs = logs.Count(l => !l.IsSuccessful),
                    LogsByAction = logs.GroupBy(l => l.Action)
                                      .ToDictionary(g => g.Key, g => g.Count()),
                    LogsLast24Hours = logs.Count(l => l.ProcessedAt >= DateTime.UtcNow.AddHours(-24)),
                    LastProcessedAt = logs.OrderByDescending(l => l.ProcessedAt)
                                         .FirstOrDefault()?.ProcessedAt
                };

                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting log statistics");
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
            return Ok(new
            {
                status = "Healthy",
                service = "NotificationService",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Crear log manualmente (para testing)
        /// </summary>
        /// <param name="request">Datos del log</param>
        /// <returns>Log creado</returns>
        [HttpPost("test")]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(InventoryLog))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<InventoryLog>> CreateTestLog([FromBody] CreateTestLogRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Creating test log for product {ProductId}", request.ProductId);

                var log = await _logService.CreateLogAsync(
                    request.ProductId,
                    request.Action,
                    request.ProductData);

                return CreatedAtAction(nameof(GetLogsByProductId),
                    new { productId = request.ProductId },
                    log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test log");
                return StatusCode(500, "Error interno del servidor");
            }
        }
    }

    public class CreateTestLogRequest
    {
        public int ProductId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string ProductData { get; set; } = string.Empty;
    }
}