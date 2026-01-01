using Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Rewards
{
    public sealed class RedemptionCodeGenerator : IRedemptionCodeGenerator
    {
        private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public string GeneratePromoCode()
        {
            return GenerateReadableCode(3, 4); // ABCD-EFGH-IJKL
        }

        public string GenerateQrSecret()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .Replace("+", "")
                .Replace("/", "")
                .Replace("=", "");
        }

        public string Hash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }

        private static string GenerateReadableCode(int groups, int charsPerGroup)
        {
            var bytes = RandomNumberGenerator.GetBytes(groups * charsPerGroup);
            var chars = bytes
                .Select(b => Alphabet[b % Alphabet.Length])
                .ToArray();

            return string.Join(
                "-",
                chars.Chunk(charsPerGroup).Select(c => new string(c)));
        }
    }
}
