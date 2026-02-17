using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces.Realtime;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class SensorController : ControllerBase
    {
        private readonly ISensorDataService _sensorService;
        private readonly ILogger<SensorController> _logger;

        public SensorController(
            ISensorDataService sensorService,
            ILogger<SensorController> logger)
        {
            _sensorService = sensorService;
            _logger = logger;
        }

        /// <summary>
        /// Dashboard snapshot — called on page load / reconnect.
        ///
        /// Returns (all from cache, no DB hit):
        ///   - LatestReading  : last known telemetry
        ///   - DeviceState    : fan + pump state
        ///   - ActiveAlerts   : threshold-based alerts from the latest reading
        /// </summary>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                _logger.LogInformation("📊 GET /api/sensor/dashboard");
                var data = await _sensorService.GetDashboardDataAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving dashboard data");
                return StatusCode(500, new { error = "Error retrieving dashboard data", message = ex.Message });
            }
        }

        [HttpGet("fire/status")]
        [ProducesResponseType(typeof(Application.DTOs.Realtime.FireStatusDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFireStatus()
        {
            try
            {
                _logger.LogInformation("🔥 GET /api/sensor/fire/status");
                var status = await _sensorService.GetFireStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving fire status");
                return StatusCode(500, new { error = "Error retrieving fire status", message = ex.Message });
            }
        }

        /// <summary>Health check.</summary>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            return Ok(new
            {
                success = true,
                status = "healthy",
                service = "SensorAPI",
                timestamp = DateTime.UtcNow,
                version = "2.0.0",
                features = new
                {
                    telemetryCache = "enabled",
                    deviceState = "enabled",
                    signalR = "enabled",
                    fireAlerts = "next sprint"
                }
            });
        }
    }
}
