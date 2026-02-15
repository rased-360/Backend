using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;

namespace RaSed.Infrastructure.Services.Realtime
{
    /// <summary>
    /// Processes raw telemetry: computes HeatIndex and checks threshold-based alerts.
    /// Air quality data (pm2_5, pm1_0, tvoc, eco2) is passed through without modification.
    /// </summary>
    public class SensorDataProcessor : ISensorDataProcessor
    {
        private readonly ILogger<SensorDataProcessor> _logger;
        private readonly AlertThresholds _thresholds;

        public SensorDataProcessor(
            ILogger<SensorDataProcessor> logger,
            IOptions<AlertThresholds> thresholds)
        {
            _logger = logger;
            _thresholds = thresholds.Value;
        }

        // ── Process ───────────────────────────────────────────────────────────

        /// <summary>
        /// Computes derived fields (HeatIndex) and validates the reading.
        /// Does NOT modify alert flags — that is done by CheckForAlerts.
        /// </summary>
        public SensorReadingDto ProcessReading(SensorReadingDto reading)
        {
            reading.HeatIndex = CalculateHeatIndex(reading.Temperature, reading.Humidity);

            if (!IsValidReading(reading))
            {
                _logger.LogWarning(
                    "⚠️ Potentially invalid sensor reading at {Timestamp} — Temp={Temp}, Hum={Hum}, Pres={Pres}",
                    reading.Timestamp, reading.Temperature, reading.Humidity, reading.Pressure);
            }

            return reading;
        }

        // ── Threshold Alerts ──────────────────────────────────────────────────

        /// <summary>
        /// Returns a list of threshold-based AlertDtos.
        /// Also sets HasAlert / AlertType / AlertMessage on the reading for convenience.
        /// </summary>
        public List<AlertDto> CheckForAlerts(SensorReadingDto reading)
        {
            var alerts = new List<AlertDto>();

            // ── Temperature ───────────────────────────────────────────────────
            if (reading.Temperature > _thresholds.TemperatureHigh)
            {
                alerts.Add(Build(
                    "TemperatureHigh",
                    $"⚠️ Temperature too high: {reading.Temperature}°C (limit: {_thresholds.TemperatureHigh}°C)",
                    "Warning", reading.Timestamp));

                _logger.LogWarning("🌡️ High temperature alert: {Temp}°C", reading.Temperature);
            }
            else if (reading.Temperature < _thresholds.TemperatureLow)
            {
                alerts.Add(Build(
                    "TemperatureLow",
                    $"❄️ Temperature too low: {reading.Temperature}°C (limit: {_thresholds.TemperatureLow}°C)",
                    "Info", reading.Timestamp));
            }

            // ── Humidity ──────────────────────────────────────────────────────
            if (reading.Humidity > _thresholds.HumidityHigh)
            {
                alerts.Add(Build(
                    "HumidityHigh",
                    $"💧 Humidity too high: {reading.Humidity}% (limit: {_thresholds.HumidityHigh}%)",
                    "Warning", reading.Timestamp));
            }
            else if (reading.Humidity < _thresholds.HumidityLow)
            {
                alerts.Add(Build(
                    "HumidityLow",
                    $"🏜️ Humidity too low: {reading.Humidity}% (limit: {_thresholds.HumidityLow}%)",
                    "Info", reading.Timestamp));
            }

            // ── Hydrogen (safety-critical) ────────────────────────────────────
            if (reading.Hydrogen > _thresholds.HydrogenHigh)
            {
                alerts.Add(Build(
                    "HydrogenHigh",
                    $"🚨 CRITICAL: Hydrogen dangerously high: {reading.Hydrogen} (limit: {_thresholds.HydrogenHigh})",
                    "Critical", reading.Timestamp));

                _logger.LogCritical("🚨 High hydrogen detected: {Level}", reading.Hydrogen);
            }

            // ── Pressure ─────────────────────────────────────────────────────
            if (reading.Pressure > _thresholds.PressureHigh || reading.Pressure < _thresholds.PressureLow)
            {
                alerts.Add(Build(
                    "PressureAbnormal",
                    $"🌀 Pressure abnormal: {reading.Pressure} (range: {_thresholds.PressureLow}–{_thresholds.PressureHigh})",
                    "Warning", reading.Timestamp));
            }

            // ── Stamp the reading so downstream consumers know at a glance ────
            if (alerts.Count > 0)
            {
                reading.HasAlert = true;
                reading.AlertType = alerts[0].Type;
                reading.AlertMessage = alerts[0].Message;
            }

            return alerts;
        }

        // ── Heat Index (NOAA formula) ─────────────────────────────────────────

        public static decimal CalculateHeatIndex(decimal temperatureC, decimal humidity)
        {
            var t = temperatureC * 9m / 5m + 32m; // °C → °F
            var rh = humidity;

            var hiF =
                -42.379m
                + 2.04901523m * t
                + 10.14333127m * rh
                - 0.22475541m * t * rh
                - 0.00683783m * t * t
                - 0.05481717m * rh * rh
                + 0.00122874m * t * t * rh
                + 0.00085282m * t * rh * rh
                - 0.00000199m * t * t * rh * rh;

            return Math.Round((hiF - 32m) * 5m / 9m, 2); // °F → °C
        }

        // ── Validation ────────────────────────────────────────────────────────

        private static bool IsValidReading(SensorReadingDto r) =>
            r.Temperature >= -50 && r.Temperature <= 100 &&
            r.Humidity >= 0 && r.Humidity <= 100 &&
            r.Pressure >= 0 && r.Pressure <= 120000 &&
            r.Hydrogen >= 0 &&
            r.Ethanol >= 0 &&
            r.Pm2_5 >= 0 &&
            r.Pm1_0 >= 0 &&
            r.Tvoc >= 0 &&
            r.Eco2 >= 0;

        // ── Helpers ───────────────────────────────────────────────────────────

        private static AlertDto Build(string type, string message, string severity, DateTime ts) =>
            new AlertDto { Type = type, Message = message, Severity = severity, Timestamp = ts };
    }
}
