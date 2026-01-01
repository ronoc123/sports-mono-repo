namespace Application.Common.Interfaces
{
    public interface IQrCodeGenerator
    {
        byte[] GeneratePng(string payload);
        string GenerateBase64(string payload);
    }
}
