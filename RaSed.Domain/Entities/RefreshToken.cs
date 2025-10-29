using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }

        public int UserId { get; set; }

        public DateTime Expires { get; set; }

        public DateTime Created { get; set; }

        public string CreatedByIp { get; set; }

        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; }

        public string? ReplacedByToken { get; set; }

        public string? ReasonRevoked { get; set; }

        // Computed properties
        public bool IsActive => Revoked == null && DateTime.UtcNow < Expires;
        public bool IsExpired => DateTime.UtcNow >= Expires;

        // Navigation
        public ApplicationUser User { get; set; } = null!;
    }
}
