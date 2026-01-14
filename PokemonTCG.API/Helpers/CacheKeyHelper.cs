using System.Security.Cryptography;
using System.Text;

namespace PokemonTCG.API.Helpers
{
    public static class CacheKeyHelper
    {
        public static string Hash(params object?[] values)
        {
            var raw = string.Join("|", values.Select(v => v?.ToString()?.Trim().ToLower() ?? "-"));
            using var sha = SHA256.Create();
            return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(raw)));
        }
    }

}
