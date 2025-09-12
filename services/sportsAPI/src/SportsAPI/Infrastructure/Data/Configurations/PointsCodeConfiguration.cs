using Domain.PointsCode;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PointsCodeConfiguration : IEntityTypeConfiguration<PointsCode>
{
  public void Configure(EntityTypeBuilder<PointsCode> b)
  {
    b.ToTable("PointsCodes");

    b.HasKey(x => x.Id);

    b.Property(x => x.Id)
     .ValueGeneratedNever()
     .HasConversion(v => v.Value, v => PointCodeId.Of(v));

    b.Property(x => x.OrganizationId)
     .HasConversion(v => v.Value, v => OrganizationId.Of(v))
     .IsRequired();

    b.Property(x => x.AssignedUserId)
     .HasConversion(v => v.Value, v => UserId.Of(v))
     .IsRequired(false);

    b.Property(x => x.Code)
     .HasMaxLength(64)      // pick a limit you like
     .IsRequired();

    b.HasIndex(x => x.Code).IsUnique();   // one code → one row

    b.Property(x => x.Points).IsRequired();

    b.Property(x => x.Status)
     .HasConversion<int>()  // store enum as int (or .HasConversion<string>())
     .IsRequired();

    b.Property(x => x.ExpiresAt);
    b.Property(x => x.RedeemedAt);
    b.Property(x => x.CreatedAt).IsRequired();

    // Optimistic concurrency to stop double-redeem races
    b.Property<byte[]>("RowVersion").IsRowVersion();
  }
}
