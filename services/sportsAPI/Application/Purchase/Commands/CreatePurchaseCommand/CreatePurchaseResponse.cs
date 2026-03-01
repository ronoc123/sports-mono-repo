namespace Application.Purchase.Commands.CreatePurchaseCommand
{
    public record CreatePurchaseResponse(Guid PurchaseId, string ClientSecret);
}
