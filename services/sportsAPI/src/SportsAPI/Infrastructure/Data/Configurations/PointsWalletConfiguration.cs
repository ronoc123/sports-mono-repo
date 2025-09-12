using Domain.PointsWallet;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PointsWalletConfiguration : IEntityTypeConfiguration<PointsWallet>
{
  public void Configure(EntityTypeBuilder<PointsWallet> b)
  {
    b.ToTable("PointsWallets");

    b.HasKey(x => x.Id);

    b.Property(x => x.Id)
     .ValueGeneratedNever()
     .HasConversion(v => v.Value, v => PointsWalletId.Of(v));

    b.Property(x => x.UserId)
     .HasConversion(v => v.Value, v => UserId.Of(v))
     .IsRequired();

    b.Property(x => x.OrganizationId)
     .HasConversion(v => v.Value, v => OrganizationId.Of(v))
     .IsRequired();

    // users can have a different balance per org
    b.HasIndex(x => new { x.UserId, x.OrganizationId }).IsUnique();

    b.Property(x => x.Balance).IsRequired();
    b.Property(x => x.CreatedAt).IsRequired();

    // Optional: protect against double-spend
    b.Property<byte[]>("RowVersion").IsRowVersion();
  }
}
