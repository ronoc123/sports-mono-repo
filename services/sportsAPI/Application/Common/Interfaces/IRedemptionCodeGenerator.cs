namespace Application.Common.Interfaces
{
    public interface IRedemptionCodeGenerator
    {
        string GeneratePromoCode();
        string GenerateQrSecret();
        string Hash(string input);
    }
}
