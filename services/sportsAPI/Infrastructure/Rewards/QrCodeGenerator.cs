using Application.Common.Interfaces;
using QRCoder;

namespace Infrastructure.Rewards
{
    public sealed class QrCodeGenerator : IQrCodeGenerator
    {
        public byte[] GeneratePng(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(data);

            return qrCode.GetGraphic(20);
        }

        public string GenerateBase64(string payload)
        {
            using var generator = new QRCodeGenerator();
            using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(data);

            return qrCode.GetGraphic(20);
        }
    }
}
