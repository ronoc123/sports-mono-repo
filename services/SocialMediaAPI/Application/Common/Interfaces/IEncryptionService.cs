namespace Application.Common.Interfaces;

public interface IEncryptionService
{
    string Encrypt(string plaintext, out string iv);
    string Decrypt(string ciphertext, string iv);
}
