using System.Security.Cryptography;

namespace IdentityService.Services;

/// <summary>
/// Singleton that loads the RSA key pair once at startup.
/// PrivateKey signs tokens; PublicKey verifies them (used for refresh validation and JWKS).
/// </summary>
public sealed class TokenKeyProvider : IDisposable
{
    public RSA PrivateKey { get; }
    public RSA PublicKey { get; }
    public string KeyId { get; } = "sportify-rsa-1";

    public TokenKeyProvider(IWebHostEnvironment env)
    {
        var keysDir = Path.Combine(env.ContentRootPath, "Keys");

        PrivateKey = RSA.Create();
        PrivateKey.ImportFromPem(File.ReadAllText(Path.Combine(keysDir, "private.pem")));

        PublicKey = RSA.Create();
        PublicKey.ImportFromPem(File.ReadAllText(Path.Combine(keysDir, "public.pem")));
    }

    public void Dispose()
    {
        PrivateKey.Dispose();
        PublicKey.Dispose();
    }
}
