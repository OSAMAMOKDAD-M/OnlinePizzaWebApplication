using System.Security.Cryptography;
using System.Text;

namespace OnlinePizzaWebApplication.Services
{
    public interface IDeviceProvider
    {
        string GetDeviceId();
        string GetDeviceName();
    }

    /// <summary>
    /// Produces a stable fingerprint for the machine/instance running the system.
    /// A random per-instance GUID is persisted under App_Data and combined with the
    /// machine name, so re-installs on another machine yield a different device id.
    /// </summary>
    public class DeviceProvider : IDeviceProvider
    {
        private readonly string _idFilePath;
        private string? _cachedId;

        public DeviceProvider(IWebHostEnvironment env)
        {
            var dir = Path.Combine(env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            _idFilePath = Path.Combine(dir, "device.id");
        }

        public string GetDeviceName() => Environment.MachineName;

        public string GetDeviceId()
        {
            if (_cachedId != null)
            {
                return _cachedId;
            }

            string seed;
            if (File.Exists(_idFilePath))
            {
                seed = File.ReadAllText(_idFilePath).Trim();
            }
            else
            {
                seed = Guid.NewGuid().ToString("N");
                File.WriteAllText(_idFilePath, seed);
            }

            var raw = $"{seed}|{Environment.MachineName}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            _cachedId = Convert.ToHexString(hash);
            return _cachedId;
        }
    }
}
