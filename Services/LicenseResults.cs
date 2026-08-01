namespace OnlinePizzaWebApplication.Services
{
    public enum LicenseState
    {
        /// <summary>No key has ever been activated on this device.</summary>
        NotActivated,
        /// <summary>A valid, non-expired license is active.</summary>
        Valid,
        /// <summary>The active license has passed its expiry date.</summary>
        Expired,
        /// <summary>The active license was disabled/revoked by an administrator.</summary>
        Disabled,
        /// <summary>The system clock appears to have been moved backwards.</summary>
        ClockTampered
    }

    public class LicenseStatusResult
    {
        public LicenseState State { get; set; } = LicenseState.NotActivated;
        public DateTime? ExpiresAtUtc { get; set; }
        public DateTime? ActivatedAtUtc { get; set; }
        public string? MaskedKey { get; set; }

        public bool IsValid => State == LicenseState.Valid;

        public int DaysRemaining => ExpiresAtUtc.HasValue
            ? (int)Math.Ceiling((ExpiresAtUtc.Value - DateTime.UtcNow).TotalDays)
            : 0;

        public int DaysExpired => ExpiresAtUtc.HasValue && DateTime.UtcNow > ExpiresAtUtc.Value
            ? (int)Math.Ceiling((DateTime.UtcNow - ExpiresAtUtc.Value).TotalDays)
            : 0;

        public bool IsNearExpiry(int warningDays) =>
            IsValid && ExpiresAtUtc.HasValue && DaysRemaining <= warningDays;
    }

    public class ActivationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? ExpiresAtUtc { get; set; }

        public static ActivationResult Fail(string message) => new() { Success = false, Message = message };
        public static ActivationResult Ok(string message, DateTime? expires) =>
            new() { Success = true, Message = message, ExpiresAtUtc = expires };
    }
}
