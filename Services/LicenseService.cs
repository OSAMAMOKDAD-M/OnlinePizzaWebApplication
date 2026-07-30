using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OnlinePizzaWebApplication.Data;
using OnlinePizzaWebApplication.Models;

namespace OnlinePizzaWebApplication.Services
{
    public interface ILicenseService
    {
        /// <summary>Creates and persists a new license key. Returns the raw key (shown once).</summary>
        Task<(LicenseKey license, string rawKey)> CreateLicenseAsync(
            LicenseDuration duration, int maxDevices, bool singleDevice, string? notes);

        /// <summary>Validates and activates a key on the current device.</summary>
        Task<ActivationResult> ActivateAsync(string rawKey, string? ipAddress);

        /// <summary>Current license status for this device (cached briefly by callers).</summary>
        Task<LicenseStatusResult> GetStatusAsync();

        Task<List<LicenseKey>> SearchAsync(string? term);
        Task<LicenseKey?> GetByIdAsync(int id);
        Task SetStatusAsync(int id, LicenseStatus status);
        Task DeleteAsync(int id);
        Task<List<LicenseActivation>> GetActivationsAsync(int licenseId);
        Task RemoveActivationAsync(int activationId);

        string DurationLabel(LicenseDuration duration);
        int WarningDays { get; }
    }

    public class LicenseService : ILicenseService
    {
        private readonly AppDbContext _context;
        private readonly IDeviceProvider _device;
        private readonly LicenseOptions _options;
        private readonly string _heartbeatPath;

        public LicenseService(
            AppDbContext context,
            IDeviceProvider device,
            IOptions<LicenseOptions> options,
            IWebHostEnvironment env)
        {
            _context = context;
            _device = device;
            _options = options.Value;
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _heartbeatPath = Path.Combine(dir, "heartbeat.dat");
        }

        public int WarningDays => _options.WarningDays;

        private string Secret =>
            string.IsNullOrWhiteSpace(_options.SigningSecret)
                ? throw new InvalidOperationException("License:SigningSecret is not configured.")
                : _options.SigningSecret;

        public static TimeSpan ToTimeSpan(LicenseDuration duration) => duration switch
        {
            LicenseDuration.Week => TimeSpan.FromDays(7),
            LicenseDuration.Month => TimeSpan.FromDays(30),
            LicenseDuration.TwoMonths => TimeSpan.FromDays(60),
            LicenseDuration.ThreeMonths => TimeSpan.FromDays(90),
            LicenseDuration.SixMonths => TimeSpan.FromDays(180),
            LicenseDuration.Year => TimeSpan.FromDays(365),
            _ => TimeSpan.FromDays(30)
        };

        public string DurationLabel(LicenseDuration duration) => duration switch
        {
            LicenseDuration.Week => "أسبوع",
            LicenseDuration.Month => "شهر",
            LicenseDuration.TwoMonths => "شهران",
            LicenseDuration.ThreeMonths => "ثلاثة أشهر",
            LicenseDuration.SixMonths => "ستة أشهر",
            LicenseDuration.Year => "سنة",
            _ => duration.ToString()
        };

        public async Task<(LicenseKey license, string rawKey)> CreateLicenseAsync(
            LicenseDuration duration, int maxDevices, bool singleDevice, string? notes)
        {
            string rawKey = LicenseCrypto.GenerateKey(Secret);
            var license = new LicenseKey
            {
                KeyHash = LicenseCrypto.HashKey(rawKey, Secret),
                MaskedKey = LicenseCrypto.Mask(rawKey),
                Duration = duration,
                Status = LicenseStatus.Active,
                CreatedAtUtc = DateTime.UtcNow,
                MaxDevices = singleDevice ? 1 : Math.Max(1, maxDevices),
                SingleDevice = singleDevice,
                Notes = notes
            };
            _context.LicenseKeys.Add(license);
            await _context.SaveChangesAsync();
            return (license, rawKey);
        }

        public async Task<ActivationResult> ActivateAsync(string rawKey, string? ipAddress)
        {
            rawKey = (rawKey ?? string.Empty).Trim();
            var masked = string.IsNullOrEmpty(rawKey) ? null : LicenseCrypto.Mask(rawKey);
            var deviceId = _device.GetDeviceId();

            async Task<ActivationResult> LogAndReturn(bool success, string reason)
            {
                _context.LicenseAttempts.Add(new LicenseAttempt
                {
                    MaskedKey = masked,
                    DeviceId = deviceId,
                    IpAddress = ipAddress,
                    Success = success,
                    Reason = reason
                });
                await _context.SaveChangesAsync();
                return success ? ActivationResult.Ok(reason, null) : ActivationResult.Fail(reason);
            }

            if (string.IsNullOrWhiteSpace(rawKey) || !LicenseCrypto.VerifyKey(rawKey, Secret))
            {
                return await LogAndReturn(false, "مفتاح غير صالح أو مزيّف.");
            }

            var hash = LicenseCrypto.HashKey(rawKey, Secret);
            var license = await _context.LicenseKeys
                .Include(l => l.Activations)
                .FirstOrDefaultAsync(l => l.KeyHash == hash);

            if (license == null)
            {
                return await LogAndReturn(false, "المفتاح غير موجود.");
            }
            if (license.Status == LicenseStatus.Disabled)
            {
                return await LogAndReturn(false, "تم تعطيل هذا المفتاح.");
            }

            var now = DateTime.UtcNow;
            if (license.ActivatedAtUtc == null)
            {
                license.ActivatedAtUtc = now;
                license.ExpiresAtUtc = now.Add(ToTimeSpan(license.Duration));
            }

            if (license.ExpiresAtUtc.HasValue && now > license.ExpiresAtUtc.Value)
            {
                license.Status = LicenseStatus.Expired;
                await _context.SaveChangesAsync();
                return await LogAndReturn(false, "انتهت صلاحية هذا المفتاح.");
            }

            var existing = license.Activations.FirstOrDefault(a => a.DeviceId == deviceId);
            if (existing == null)
            {
                if (license.Activations.Count >= license.MaxDevices)
                {
                    return await LogAndReturn(false, "تم بلوغ الحد الأقصى للأجهزة المسموح بها لهذا المفتاح.");
                }
                _context.LicenseActivations.Add(new LicenseActivation
                {
                    LicenseKeyId = license.Id,
                    DeviceId = deviceId,
                    DeviceName = _device.GetDeviceName(),
                    ActivatedAtUtc = now,
                    LastSeenUtc = now
                });
            }
            else
            {
                existing.LastSeenUtc = now;
            }

            license.Status = LicenseStatus.Active;
            await _context.SaveChangesAsync();
            WriteHeartbeat(now);
            return ActivationResult.Ok("تم تفعيل الاشتراك بنجاح.", license.ExpiresAtUtc);
        }

        public async Task<LicenseStatusResult> GetStatusAsync()
        {
            var deviceId = _device.GetDeviceId();
            var now = DateTime.UtcNow;

            if (IsClockTampered(now))
            {
                return new LicenseStatusResult { State = LicenseState.ClockTampered };
            }

            var activation = await _context.LicenseActivations
                .Include(a => a.LicenseKey)
                .Where(a => a.DeviceId == deviceId && a.LicenseKey != null)
                .OrderByDescending(a => a.LicenseKey!.ExpiresAtUtc)
                .FirstOrDefaultAsync();

            if (activation?.LicenseKey == null)
            {
                return new LicenseStatusResult { State = LicenseState.NotActivated };
            }

            var license = activation.LicenseKey;
            var result = new LicenseStatusResult
            {
                ExpiresAtUtc = license.ExpiresAtUtc,
                ActivatedAtUtc = license.ActivatedAtUtc,
                MaskedKey = license.MaskedKey
            };

            if (license.Status == LicenseStatus.Disabled)
            {
                result.State = LicenseState.Disabled;
                return result;
            }

            if (license.ExpiresAtUtc.HasValue && now > license.ExpiresAtUtc.Value)
            {
                if (license.Status != LicenseStatus.Expired)
                {
                    license.Status = LicenseStatus.Expired;
                    await _context.SaveChangesAsync();
                }
                result.State = LicenseState.Expired;
                return result;
            }

            activation.LastSeenUtc = now;
            await _context.SaveChangesAsync();
            WriteHeartbeat(now);
            result.State = LicenseState.Valid;
            return result;
        }

        public Task<List<LicenseKey>> SearchAsync(string? term)
        {
            var query = _context.LicenseKeys
                .Include(l => l.Activations)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(term))
            {
                term = term.Trim();
                query = query.Where(l =>
                    l.MaskedKey.Contains(term) ||
                    (l.Notes != null && l.Notes.Contains(term)));
            }
            return query.OrderByDescending(l => l.CreatedAtUtc).ToListAsync();
        }

        public Task<LicenseKey?> GetByIdAsync(int id) =>
            _context.LicenseKeys.Include(l => l.Activations).FirstOrDefaultAsync(l => l.Id == id);

        public async Task SetStatusAsync(int id, LicenseStatus status)
        {
            var license = await _context.LicenseKeys.FindAsync(id);
            if (license != null)
            {
                license.Status = status;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var license = await _context.LicenseKeys.FindAsync(id);
            if (license != null)
            {
                _context.LicenseKeys.Remove(license);
                await _context.SaveChangesAsync();
            }
        }

        public Task<List<LicenseActivation>> GetActivationsAsync(int licenseId) =>
            _context.LicenseActivations
                .Where(a => a.LicenseKeyId == licenseId)
                .OrderByDescending(a => a.ActivatedAtUtc)
                .ToListAsync();

        public async Task RemoveActivationAsync(int activationId)
        {
            var activation = await _context.LicenseActivations.FindAsync(activationId);
            if (activation != null)
            {
                _context.LicenseActivations.Remove(activation);
                await _context.SaveChangesAsync();
            }
        }

        // --- Clock tamper detection -------------------------------------------------

        private bool IsClockTampered(DateTime now)
        {
            try
            {
                if (!File.Exists(_heartbeatPath))
                {
                    return false;
                }
                var text = File.ReadAllText(_heartbeatPath).Trim();
                if (long.TryParse(text, out var ticks))
                {
                    var last = new DateTime(ticks, DateTimeKind.Utc);
                    return now < last.AddMinutes(-_options.ClockToleranceMinutes);
                }
            }
            catch
            {
                // ignore heartbeat read failures
            }
            return false;
        }

        private void WriteHeartbeat(DateTime now)
        {
            try
            {
                File.WriteAllText(_heartbeatPath, now.Ticks.ToString());
            }
            catch
            {
                // best-effort
            }
        }
    }
}
