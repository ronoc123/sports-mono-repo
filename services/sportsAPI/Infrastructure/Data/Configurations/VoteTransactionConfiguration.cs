using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data.VM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class VoteTransactionConfiguration : IEntityTypeConfiguration<VoteTransaction>
{
  public void Configure(EntityTypeBuilder<VoteTransaction> builder)
  {
    builder.ToTable("vote_transactions");

    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
        .HasColumnName("id")
        .ValueGeneratedOnAdd();

    builder.Property(x => x.OrgId)
        .HasColumnName("org_id")
        .HasConversion(id => id.Value, value => OrganizationId.Of(value))
        .IsRequired();

    builder.Property(x => x.UserId)
        .HasColumnName("user_id")
        .HasConversion(id => id.Value, value => UserId.Of(value))
        .IsRequired();

    builder.Property(x => x.Amount)
        .HasColumnName("amount")
        .IsRequired();

    builder.Property(x => x.Reason)
        .HasColumnName("reason")
        .HasMaxLength(100) // adjust as you like
        .IsRequired();

    builder.Property(x => x.RefId)
        .HasColumnName("ref_id");

    builder.Property(x => x.CreatedAt)
      .HasColumnName("created_at")
      .HasColumnType("datetimeoffset")
      .HasDefaultValueSql("SYSUTCDATETIME()") // ✅ SQL Server
      .IsRequired();

    // Index on (org_id, user_id, created_at)
    builder.HasIndex(x => new { x.OrgId, x.UserId, x.CreatedAt })
           .HasDatabaseName("ix_vt_org_user_time");

    // FKs (optional)
    builder.HasOne<Domain.Organizations.Organization>()
           .WithMany()
           .HasForeignKey(nameof(VoteTransaction.OrgId))
           .OnDelete(DeleteBehavior.Restrict);
  }
}
