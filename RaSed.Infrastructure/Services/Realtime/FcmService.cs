using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Enums;

namespace RaSed.Infrastructure.Services.Realtime
{
    /// <summary>
    /// Firebase Cloud Messaging implementation.
    /// 
    /// GENERIC DESIGN: Supports any notification type via topics.
    /// Current topics: fire-alerts
    /// Future topics: maintenance-alerts, shift-reminders, equipment-failures, etc.
    /// 
    /// SETUP:
    /// 1. Add firebase-adminsdk.json to project root
    /// 2. Install: dotnet add package FirebaseAdmin
    /// 3. Register in DI: services.AddSingleton&lt;IFcmService, FcmService&gt;();
    /// </summary>
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly FirebaseMessaging _messaging;

        public FcmService(ILogger<FcmService> logger)
        {
            _logger = logger;

            // Initialize Firebase App (once, on first service instantiation)
            if (FirebaseApp.DefaultInstance == null)
            {
                var credentialPath = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "firebase-adminsdk.json"
                );

                if (!File.Exists(credentialPath))
                {
                    _logger.LogError("❌ Firebase credentials not found at: {Path}", credentialPath);
                    throw new FileNotFoundException($"Firebase credentials file not found: {credentialPath}");
                }

                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(credentialPath)
                });

                _logger.LogInformation("✅ Firebase initialized with credentials from: {Path}", credentialPath);
            }

            _messaging = FirebaseMessaging.DefaultInstance;
        }

        /// <summary>
        /// Sends a notification to all devices subscribed to the specified topic.
        /// 
        /// FLEXIBLE: Works for any notification type — fire alerts, maintenance, reminders, etc.
        /// 
        /// The data dictionary can contain any key-value pairs for app routing/logic.
        /// Common data keys:
        ///   - "type": notification category (fire_started, maintenance, etc.)
        ///   - "id": relevant entity ID (event_id, equipment_id, etc.)
        ///   - "timestamp": ISO 8601 timestamp
        ///   - "action": suggested app action (navigate_to_screen, show_dialog, etc.)
        /// </summary>
        public async Task SendToTopicAsync(
            string topic,
            string title,
            string body,
            Dictionary<string, string>? data = null,
            FcmNotificationPriority priority = FcmNotificationPriority.High,
            string? color = null)
        {
            try
            {
                var message = new Message
                {
                    Topic = topic,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = data ?? new Dictionary<string, string>(),

                    // Android-specific configuration
                    Android = new AndroidConfig
                    {
                        Priority = MapToAndroidPriority(priority),
                        Notification = new AndroidNotification
                        {
                            ChannelId = MapToChannelId(topic),
                            Priority = MapToAndroidNotificationPriority(priority),
                            Color = color,
                            Sound = "default",
                            DefaultSound = true,
                            DefaultVibrateTimings = true,
                            DefaultLightSettings = true
                        }
                    },

                    // iOS-specific configuration
                    Apns = new ApnsConfig
                    {
                        Aps = new Aps
                        {
                            Alert = new ApsAlert
                            {
                                Title = title,
                                Body = body
                            },
                            Sound = "default",
                            Badge = 1,
                            ContentAvailable = true
                        }
                    }
                };

                var response = await _messaging.SendAsync(message);

                _logger.LogInformation(
                    "📤 FCM sent — Topic={Topic}, Priority={Priority}, Response={Response}",
                    topic, priority, response
                );
            }
            catch (FirebaseMessagingException fex)
            {
                _logger.LogError(fex,
                    "❌ FCM error — Topic={Topic}, Code={Code}, Message={Message}",
                    topic, fex.MessagingErrorCode, fex.Message
                );

                // Don't rethrow — FCM failure should never crash the pipeline
                // The SignalR notification will still work for connected clients
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected FCM error — Topic={Topic}", topic);
            }
        }

        // ── Priority Mapping ──────────────────────────────────────────────────

        private static Priority MapToAndroidPriority(FcmNotificationPriority priority) =>
            priority switch
            {
                FcmNotificationPriority.Critical => Priority.High,
                FcmNotificationPriority.High => Priority.High,
                FcmNotificationPriority.Normal => Priority.Normal,
                FcmNotificationPriority.Low => Priority.Normal,
                _ => Priority.High
            };

        private static NotificationPriority MapToAndroidNotificationPriority(FcmNotificationPriority priority) =>
            priority switch
            {
                FcmNotificationPriority.Critical => NotificationPriority.MAX,
                FcmNotificationPriority.High => NotificationPriority.HIGH,
                FcmNotificationPriority.Normal => NotificationPriority.DEFAULT,
                FcmNotificationPriority.Low => NotificationPriority.LOW,
                _ => NotificationPriority.HIGH
            };

        // ── Channel ID Mapping ────────────────────────────────────────────────

        /// <summary>
        /// Maps topics to Android notification channel IDs.
        /// Each topic should have a dedicated channel for proper notification management.
        /// 
        /// Flutter must create matching channels with the same IDs.
        /// </summary>
        private static string MapToChannelId(string topic) =>
            topic switch
            {
                "fire-alerts" => "fire_alerts",
                "maintenance-alerts" => "maintenance_alerts",
                "shift-reminders" => "shift_reminders",
                "equipment-failures" => "equipment_failures",
                _ => "default_notifications"
            };
    }
}