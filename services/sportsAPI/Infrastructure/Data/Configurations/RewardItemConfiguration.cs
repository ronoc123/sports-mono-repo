using Domain.Rewards;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class RewardItemConfiguration : IEntityTypeConfiguration<RewardItem>
{
  public void Configure(EntityTypeBuilder<RewardItem> builder)
  {
    // Table name
    builder.ToTable("RewardItems");

    // Primary Key
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasConversion(
            id => id.Value,
            value =>  RewardItemId.Of(value))
        .ValueGeneratedNever();

    // OrganizationId (Value Object)
    builder.Property(x => x.OrganizationId)
        .HasConversion(
            id => id.Value,
            value => OrganizationId.Of(value))
        .IsRequired();

    // VoteValue
    builder.Property(x => x.VoteValue)
        .IsRequired();

    // QR Code (string)
    builder.Property(x => x.QrCode)
        .HasColumnType("nvarchar(max)")
        .IsRequired();

    builder.Property(p => p.RedeemedBy)
    .HasConversion(
        v => v == null ? (Guid?)null : v.Value,
        v => v.HasValue ? UserId.Of(v.Value) : null)
            .IsRequired(false);

    // RedeemedAt (nullable datetime)
    builder.Property(x => x.RedeemedAt)
        .IsRequired(false);

    // Ignore computed property
    builder.Ignore(x => x.IsRedeemed);
  }
}
