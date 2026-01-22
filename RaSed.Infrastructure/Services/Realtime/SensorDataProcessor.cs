using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class SensorDataProcessor : ISensorDataProcessor
    {
        private readonly ILogger<SensorDataProcessor> _logger;
        private readonly AlertThresholds _thresholds;

        public SensorDataProcessor(ILogger<SensorDataProcessor> logger, IOptions<AlertThresholds> thresholds)
        {
            _logger = logger;
            _thresholds = thresholds.Value;
        }
        public SensorReadingDto ProcessReading(SensorReadingDto reading)
        {
            // Apply conversion formula to temperature
            reading.HeatIndex = CalculateHeatIndex(reading.Temperature,reading.Humidity);

            // Validate readings
            if (!IsValidReading(reading))
            {
                _logger.LogWarning("⚠️ Invalid sensor reading detected at {Timestamp}",
                    reading.Timestamp);
            }

            return reading;
        }
        public List<AlertDto> CheckForAlerts(SensorReadingDto reading)
        {
            var alerts = new List<AlertDto>();

            // Temperature alerts
            if (reading.Temperature > _thresholds.TemperatureHigh)
            {
                alerts.Add(new AlertDto
                {
                    Type = "TemperatureHigh",
                    Message = $"⚠️ Temperature is too high: {reading.Temperature}°C (Threshold: {_thresholds.TemperatureHigh}°C)",
                    Timestamp = reading.Timestamp,
                    Severity = "Warning"
                });
                _logger.LogWarning("🌡️ High temperature alert: {Temp}°C", reading.Temperature);
            }
            else if (reading.Temperature < _thresholds.TemperatureLow)
            {
                alerts.Add(new AlertDto
                {
                    Type = "TemperatureLow",
                    Message = $"❄️ Temperature is too low: {reading.Temperature}°C (Threshold: {_thresholds.TemperatureLow}°C)",
                    Timestamp = reading.Timestamp,
                    Severity = "Info"
                });
            }

            // Humidity alerts
            if (reading.Humidity > _thresholds.HumidityHigh)
            {
                alerts.Add(new AlertDto
                {
                    Type = "HumidityHigh",
                    Message = $"💧 Humidity is too high: {reading.Humidity}% (Threshold: {_thresholds.HumidityHigh}%)",
                    Timestamp = reading.Timestamp,
                    Severity = "Warning"
                });
            }
            else if (reading.Humidity < _thresholds.HumidityLow)
            {
                alerts.Add(new AlertDto
                {
                    Type = "HumidityLow",
                    Message = $"🏜️ Humidity is too low: {reading.Humidity}% (Threshold: {_thresholds.HumidityLow}%)",
                    Timestamp = reading.Timestamp,
                    Severity = "Info"
                });
            }

            // Hydrogen alert (safety critical!)
            if (reading.Hydrogen > _thresholds.HydrogenHigh)
            {
                alerts.Add(new AlertDto
                {
                    Type = "HydrogenHigh",
                    Message = $"🚨 CRITICAL: Hydrogen level is dangerously high: {reading.Hydrogen} (Threshold: {_thresholds.HydrogenHigh})",
                    Timestamp = reading.Timestamp,
                    Severity = "Critical"
                });
                _logger.LogCritical("🚨 CRITICAL: High hydrogen level detected: {Level}",
                    reading.Hydrogen);
            }

            // Pressure alerts
            if (reading.Pressure > _thresholds.PressureHigh ||
               reading.Pressure < _thresholds.PressureLow)
            {
                alerts.Add(new AlertDto
                {
                    Type = "PressureAbnormal",
                    Message = $"🌀 Pressure is abnormal: {reading.Pressure} (Normal range: {_thresholds.PressureLow}-{_thresholds.PressureHigh})",
                    Timestamp = reading.Timestamp,
                    Severity = "Warning"
                });
            }

            // Update reading with alert info if alerts exist
            if (alerts.Any())
            {
                reading.HasAlert = true;
                reading.AlertType = alerts.First().Type;
                reading.AlertMessage = alerts.First().Message;
            }

            return alerts;
        }

        public static decimal CalculateHeatIndex(decimal temperatureC, decimal humidity)
        {
            // 1️⃣ Convert Celsius to Fahrenheit
            var t = (temperatureC * 9 / 5) + 32;
            var rh = humidity;

            // 2️⃣ NOAA Heat Index Formula
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

            // 3️⃣ Convert back to Celsius
            var hiC = (hiF - 32) * 5 / 9;

            // 4️⃣ Round to 2 decimal places
            return Math.Round(hiC, 2);
        }

        private bool IsValidReading(SensorReadingDto reading)
        {
            // Validate sensor reading ranges
            bool isValid =
                reading.Temperature >= -50 && reading.Temperature <= 100 &&
                reading.Humidity >= 0 && reading.Humidity <= 100 &&
                reading.Pressure >= 0 && reading.Pressure <= 120000 &&
                reading.Hydrogen >= 0 &&
                reading.Ethanol >= 0;

            return isValid;
        }

    }
}
