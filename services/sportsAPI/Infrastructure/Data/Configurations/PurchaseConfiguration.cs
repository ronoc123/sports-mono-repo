using Domain.Purchase;
using Domain.ValueObjects;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PurchaseConfiguration : IEntityTypeConfiguration<Purchase>
{
    public void Configure(EntityTypeBuilder<Purchase> builder)
    {
        builder.ToTable("Purchases");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasConversion(id => id.Value, v => UserId.Of(v))
            .IsRequired();

        builder.Property(x => x.OrgId)
            .HasConversion(id => id.Value, v => OrganizationId.Of(v))
            .IsRequired();

        builder.OwnsOne(x => x.Item, item =>
        {
            item.Property(i => i.Type)
                .HasColumnName("ItemType")
                .HasMaxLength(100)
                .IsRequired();

            item.Property(i => i.Quantity)
                .HasColumnName("ItemQuantity")
                .IsRequired();
        });

        builder.OwnsOne(x => x.Price, price =>
        {
            price.Property(p => p.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            price.Property(p => p.Currency)
                .HasMaxLength(3)
                .HasColumnName("PriceCurrency")
                .IsRequired();
        });

        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.FulfillmentStatus).IsRequired();
        builder.Property(x => x.Provider).IsRequired();

        builder.Property(x => x.ExternalPaymentId)
            .HasConversion(
                id => id == null ? null : id.Value,
                v => v == null ? null : ExternalPaymentId.Create(v))
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.ExternalSessionId)
            .HasConversion(
                id => id == null ? null : id.Value,
                v => v == null ? null : ExternalSessionId.Create(v))
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(x => x.CreatedAt)
            .HasColumnType("datetime2")
            .IsRequired();

        builder.Property(x => x.PaidAt)
            .HasColumnType("datetime2")
            .IsRequired(false);
    }
}
