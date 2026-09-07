using System.Security.Cryptography;
using System.Text;
using Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services;

public class AesEncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public AesEncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["Encryption:Key"]
            ?? throw new InvalidOperationException(
                "Encryption:Key is not configured. Set a Base64-encoded 32-byte key.");
        _key = Convert.FromBase64String(keyBase64);
        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption:Key must decode to exactly 32 bytes (AES-256).");
    }

    public string Encrypt(string plaintext, out string iv)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        iv = Convert.ToBase64String(aes.IV);

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        return Convert.ToBase64String(cipherBytes);
    }

    public string Decrypt(string ciphertext, string ivBase64)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = Convert.FromBase64String(ivBase64);

        using var decryptor = aes.CreateDecryptor();
        var cipherBytes = Convert.FromBase64String(ciphertext);
        var plaintextBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
        return Encoding.UTF8.GetString(plaintextBytes);
    }
}
