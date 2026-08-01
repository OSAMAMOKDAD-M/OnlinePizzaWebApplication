using System.ComponentModel.DataAnnotations;

namespace OnlinePizzaWebApplication.Models
{
    /// <summary>Records a device that has activated a given license key.</summary>
    public class LicenseActivation
    {
        public int Id { get; set; }

        public int LicenseKeyId { get; set; }
        public LicenseKey? LicenseKey { get; set; }

        /// <summary>Stable fingerprint of the machine/instance running the system.</summary>
        [Required]
        [StringLength(128)]
        public string DeviceId { get; set; } = string.Empty;

        [StringLength(150)]
        public string? DeviceName { get; set; }

        public DateTime ActivatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Last time this device was seen with a valid license (used for clock-tamper detection).</summary>
        public DateTime LastSeenUtc { get; set; } = DateTime.UtcNow;
    }
}
