namespace Application.Common.Models;

public class PublishRequest
{
    public string ChannelId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// The encrypted refresh token stored on the LinkedAccount.
    /// Infrastructure adapters are responsible for decryption — never Application layer.
    /// </summary>
    public string EncryptedRefreshToken { get; set; } = string.Empty;
    public string TokenIv { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Hashtags { get; set; } = new();
    public string VideoPath { get; set; } = string.Empty;
}
