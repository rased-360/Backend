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
        /// Get complete dashboard data including latest reading and today's chart
        /// </summary>
        /// <returns>Dashboard data DTO</returns>
        [HttpGet("dashboard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetDashboard()
        {
            try
            {
                _logger.LogInformation("📊 GET /api/sensor/dashboard - Retrieving dashboard data");

                var data = await _sensorService.GetDashboardDataAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving dashboard data");
                return StatusCode(500, new
                {
                    error = "Error retrieving dashboard data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Get today's chart data for all sensors
        /// </summary>
        /// <returns>Chart data DTO with all sensor readings for today</returns>
        [HttpGet("chart/today")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTodayChart()
        {
            try
            {
                _logger.LogInformation("📈 GET /api/sensor/chart/today - Retrieving today's chart data");

                var data = await _sensorService.GetTodayChartDataAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving chart data");
                return StatusCode(500, new
                {
                    error = "Error retrieving chart data",
                    message = ex.Message
                });
            }
        }

        /// <summary>
        /// Health check endpoint to verify API and MQTT connection status
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet("health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult HealthCheck()
        {
            _logger.LogDebug("Health check requested");

            return Ok(new
            {
                success = true,
                status = "healthy",
                service = "SensorAPI",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                features = new
                {
                    caching = "enabled",
                    aggregation = "enabled",
                    cleanup = "enabled",
                    signalr = "enabled"
                }
            });
        }


    }
}
