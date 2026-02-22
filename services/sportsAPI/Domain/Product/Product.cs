namespace Domain.Product
{
    public sealed class Product : Aggregate<Guid>
    {
        public string Name { get; private set; } = string.Empty;
        public string Description { get; private set; } = string.Empty;

        public ProductType Type { get; private set; }
        public int Quantity { get; private set; }   // votes amount for example

        public Money Price { get; private set; } = null!;

        public bool IsActive { get; private set; }

        private Product() { }

        private Product(Guid id, string name, string description, ProductType type, int quantity, Money price)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            Quantity = quantity;
            Price = price;
            IsActive = true;
        }

        public static Product Create(string name, string description, ProductType type, int quantity, Money price)
        {
            return new Product(
                Guid.NewGuid(),
                name,
                description,
                type,
                quantity,
                price);
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }

}
