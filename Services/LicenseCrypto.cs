using System.Security.Cryptography;
using System.Text;

namespace OnlinePizzaWebApplication.Services
{
    /// <summary>
    /// Cryptographic helpers for the license subsystem.
    ///
    /// Key format (before grouping): Base32( DATA(15 bytes random) || SIG(5 bytes) )
    /// where SIG = HMAC-SHA256(DATA, secret)[..5]. This lets us reject forged keys
    /// without a DB round-trip. The raw key is never persisted; only <see cref="HashKey"/>.
    /// </summary>
    public static class LicenseCrypto
    {
        private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        private const int DataLength = 15;
        private const int SigLength = 5;

        public static string GenerateKey(string secret)
        {
            var data = RandomNumberGenerator.GetBytes(DataLength);
            var sig = Sign(data, secret);
            var payload = new byte[DataLength + SigLength];
            Buffer.BlockCopy(data, 0, payload, 0, DataLength);
            Buffer.BlockCopy(sig, 0, payload, DataLength, SigLength);
            return Group(Base32Encode(payload));
        }

        /// <summary>Verifies the embedded HMAC signature. Returns false for malformed or forged keys.</summary>
        public static bool VerifyKey(string key, string secret)
        {
            try
            {
                var payload = Base32Decode(Normalize(key));
                if (payload.Length != DataLength + SigLength)
                {
                    return false;
                }
                var data = payload[..DataLength];
                var sig = payload[DataLength..];
                var expected = Sign(data, secret);
                return CryptographicOperations.FixedTimeEquals(sig, expected);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>Salted (keyed) SHA-256 hash used to store the key at rest.</summary>
        public static string HashKey(string key, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(Normalize(key)));
            return Convert.ToHexString(hash);
        }

        public static string Mask(string key)
        {
            var groups = Normalize(key).Length >= 8
                ? key.Split('-')
                : new[] { key };
            if (groups.Length <= 2)
            {
                return "****";
            }
            var first = groups[0];
            var last = groups[^1];
            var middle = string.Join("-", Enumerable.Repeat("****", groups.Length - 2));
            return $"{first}-{middle}-{last}";
        }

        private static byte[] Sign(byte[] data, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            return hmac.ComputeHash(data)[..SigLength];
        }

        private static string Normalize(string key) =>
            key.Replace("-", string.Empty).Replace(" ", string.Empty).Trim().ToUpperInvariant();

        private static string Group(string raw)
        {
            var parts = new List<string>();
            for (int i = 0; i < raw.Length; i += 4)
            {
                parts.Add(raw.Substring(i, Math.Min(4, raw.Length - i)));
            }
            return string.Join("-", parts);
        }

        private static string Base32Encode(byte[] data)
        {
            var sb = new StringBuilder();
            int buffer = 0, bitsLeft = 0;
            foreach (var b in data)
            {
                buffer = (buffer << 8) | b;
                bitsLeft += 8;
                while (bitsLeft >= 5)
                {
                    int index = (buffer >> (bitsLeft - 5)) & 0x1F;
                    bitsLeft -= 5;
                    sb.Append(Base32Alphabet[index]);
                }
            }
            if (bitsLeft > 0)
            {
                int index = (buffer << (5 - bitsLeft)) & 0x1F;
                sb.Append(Base32Alphabet[index]);
            }
            return sb.ToString();
        }

        private static byte[] Base32Decode(string input)
        {
            int buffer = 0, bitsLeft = 0;
            var output = new List<byte>();
            foreach (var c in input)
            {
                int val = Base32Alphabet.IndexOf(c);
                if (val < 0)
                {
                    throw new FormatException("Invalid Base32 character.");
                }
                buffer = (buffer << 5) | val;
                bitsLeft += 5;
                if (bitsLeft >= 8)
                {
                    output.Add((byte)((buffer >> (bitsLeft - 8)) & 0xFF));
                    bitsLeft -= 8;
                }
            }
            return output.ToArray();
        }
    }
}
