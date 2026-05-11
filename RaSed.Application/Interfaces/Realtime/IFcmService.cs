using RaSed.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
        /// <summary>
        /// Firebase Cloud Messaging service for sending push notifications to mobile devices.
        /// 
        /// DESIGN: Generic and reusable — supports any notification type, not just fire alerts.
        /// Future use cases:  equipment failures, etc.
        /// </summary>
        public interface IFcmService
        {
            /// <summary>
            /// Sends a notification to all devices subscribed to a topic.
            /// 
            /// FLEXIBLE DESIGN:
            ///   - Any topic (fire-alerts, maintenance-alerts, etc.)
            ///   - Any title/body content
            ///   - Custom data payload for app routing/logic
            ///   - Optional priority and color
            /// 
 
            /// </summary>
            Task SendToTopicAsync(
                string topic,
                string title,
                string body,
                Dictionary<string, string>? data = null,
                FcmNotificationPriority priority = FcmNotificationPriority.High,
                string? color = null);

        /// <summary>Sends to a single device via its FCM token</summary>
            Task SendToDeviceAsync(
                string deviceToken,
                string title,
                string body,
                Dictionary<string, string>? data = null,
                FcmNotificationPriority priority = FcmNotificationPriority.High,
                string? color = null);
    }

    
}
