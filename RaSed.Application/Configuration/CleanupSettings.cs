using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Configuration
{
    public class CleanupSettings
    {
        public const string SectionName = "CleanupSettings";

        public CleanupServiceSettings Violations { get; set; } = new();
        public CleanupServiceSettings FireEvents { get; set; } = new();
        public RefreshTokenCleanupSettings RefreshTokens { get; set; } = new();
        public OtpCleanupSettings Otp { get; set; } = new();
    }

    public class CleanupServiceSettings
    {
        public int RetentionDays { get; set; } = 30;
        public int IntervalHours { get; set; } = 24;
        public int InitialDelayMinutes { get; set; } = 1;
    }

    public class RefreshTokenCleanupSettings
    {
        /// <summary>How long to keep expired tokens. Short — they can't be used.</summary>
        public int ExpiredRetentionDays { get; set; } = 1;

        /// <summary>
        /// How long to keep revoked tokens.
        /// Must be >= refresh token lifetime to guarantee reuse detection.
        /// </summary>
        public int RevokedRetentionDays { get; set; } = 30;

        public int IntervalHours { get; set; } = 24;
    }

    public class OtpCleanupSettings
    {
        public int IntervalHours { get; set; } = 1;
    }

}