using System.ComponentModel.DataAnnotations;

namespace OnlinePizzaWebApplication.Models
{
    /// <summary>Audit log for activation attempts (successful and failed) to detect abuse.</summary>
    public class LicenseAttempt
    {
        public int Id { get; set; }

        public DateTime AttemptedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Masked key, never the raw value.</summary>
        [StringLength(60)]
        public string? MaskedKey { get; set; }

        [StringLength(128)]
        public string? DeviceId { get; set; }

        [StringLength(64)]
        public string? IpAddress { get; set; }

        public bool Success { get; set; }

        [StringLength(200)]
        public string? Reason { get; set; }
    }
}
