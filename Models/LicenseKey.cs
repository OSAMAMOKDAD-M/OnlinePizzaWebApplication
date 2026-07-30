using System.ComponentModel.DataAnnotations;

namespace OnlinePizzaWebApplication.Models
{
    /// <summary>
    /// A license key. The raw key is never stored; only a salted SHA-256 hash is kept
    /// (<see cref="KeyHash"/>). Authenticity of a presented key is verified with an
    /// embedded HMAC signature so forged keys are rejected before any DB lookup.
    /// </summary>
    public class LicenseKey
    {
        public int Id { get; set; }

        /// <summary>SHA-256 hash of the raw key. Unique. The raw key is shown only once at creation.</summary>
        [Required]
        [StringLength(128)]
        public string KeyHash { get; set; } = string.Empty;

        /// <summary>Masked form for display in the admin panel, e.g. OPZA-****-****-K9F2.</summary>
        [StringLength(60)]
        public string MaskedKey { get; set; } = string.Empty;

        [Display(Name = "مدة الاشتراك")]
        public LicenseDuration Duration { get; set; }

        [Display(Name = "الحالة")]
        public LicenseStatus Status { get; set; } = LicenseStatus.Active;

        [Display(Name = "تاريخ الإنشاء")]
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Set on first activation.</summary>
        [Display(Name = "تاريخ التفعيل")]
        public DateTime? ActivatedAtUtc { get; set; }

        /// <summary>Computed automatically as ActivatedAt + Duration on first activation.</summary>
        [Display(Name = "تاريخ الانتهاء")]
        public DateTime? ExpiresAtUtc { get; set; }

        /// <summary>Maximum number of distinct devices allowed to activate this key.</summary>
        [Display(Name = "عدد الأجهزة المسموح بها")]
        [Range(1, 1000)]
        public int MaxDevices { get; set; } = 1;

        /// <summary>When true, the key is locked to a single device (no additional devices).</summary>
        [Display(Name = "قفل على جهاز واحد")]
        public bool SingleDevice { get; set; } = true;

        [StringLength(300)]
        [Display(Name = "ملاحظات")]
        public string? Notes { get; set; }

        public ICollection<LicenseActivation> Activations { get; set; } = new List<LicenseActivation>();
    }
}
