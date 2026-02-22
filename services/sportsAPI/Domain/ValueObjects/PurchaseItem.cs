namespace Domain.ValueObjects
{

    public sealed record PurchaseItem(
        string Type,
        int Quantity
    );

}
