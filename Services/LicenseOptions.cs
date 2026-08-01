namespace OnlinePizzaWebApplication.Services
{
    /// <summary>License-related configuration bound from the "License" section (no hardcoded secrets).</summary>
    public class LicenseOptions
    {
        public const string SectionName = "License";

        /// <summary>Secret used to sign/verify keys (HMAC) and hash stored keys. Must be set in configuration.</summary>
        public string SigningSecret { get; set; } = string.Empty;

        /// <summary>Warn the user this many days before expiry.</summary>
        public int WarningDays { get; set; } = 7;

        /// <summary>Tolerance (minutes) when detecting a backwards clock change.</summary>
        public int ClockToleranceMinutes { get; set; } = 120;
    }
}
