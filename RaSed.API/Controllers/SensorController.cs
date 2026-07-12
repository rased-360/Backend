using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Returns the current fire status shaped for the requesting client type.
        /// 
        /// Called ONCE on client connect/page load — SignalR takes over after that.
        /// 
        /// Header: X-Client-Type: desktop  →  returns DesktopTitle + DesktopBody
        /// Header: X-Client-Type: mobile   →  returns MobileTitle  + MobileBody
        /// 
        /// If the header is missing, defaults to desktop.
        /// </summary>
        [HttpGet("fire/status")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetFireStatus()
        {
            try
            {
                _logger.LogInformation("🔥 GET /api/sensor/fire/status");

                // Read client type from custom header — defaults to "desktop" if missing
                var clientType = Request.Headers["X-Client-Type"].ToString().ToLower();
                var isMobile = clientType == "mobile";

                var status = await _sensorService.GetFireStatusAsync();

                // Shape the response — each client only gets the content it needs
                if (isMobile)
                {
                    return Ok(new
                    {
                        deviceId = status.DeviceId,
                        type = status.Type,
                        status = status.Status,
                        timestamp = status.Timestamp,
                        title = status.MobileTitle,
                        body = status.MobileBody
                    });
                }

                // Default: desktop
                return Ok(new
                {
                    deviceId = status.DeviceId,
                    type = status.Type,
                    status = status.Status,
                    timestamp = status.Timestamp,
                    title = status.DesktopTitle,
                    body = status.DesktopBody
                });
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
