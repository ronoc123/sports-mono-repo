using Domain.VoteAccount;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class VoteAccountConfiguration : IEntityTypeConfiguration<VoteAccount>
{
  public void Configure(EntityTypeBuilder<VoteAccount> builder)
  {
    builder.ToTable("vote_accounts");

    // Aggregate Id is a composite; ignore the VO Id property if present
    builder.Ignore(x => x.Id);

    // Composite PK: one account per (org, user)
    builder.HasKey(x => new { x.OrgId, x.UserId });

    builder.Property(x => x.OrgId)
      .HasColumnName("org_id")
      .HasConversion(v => v.Value, v => OrganizationId.Of(v))
      .IsRequired();

    builder.Property(x => x.UserId)
      .HasColumnName("user_id")
      .HasConversion(v => v.Value, v => Domain.ValueObjects.ConcreteTypes.UserId.Of(v))
      .IsRequired();

    builder.Property(x => x.Balance)
      .HasColumnName("balance")
      .IsRequired();

    builder.Property(x => x.Version)
      .HasColumnName("version")
      .IsRequired()
      .IsConcurrencyToken();

    builder.Property(x => x.CreatedAt)
      .HasColumnName("created_at")
      .HasColumnType("datetime2")
      .HasDefaultValueSql("SYSUTCDATETIME()")
      .IsRequired();

    builder.Property(x => x.UpdatedAt)
      .HasColumnName("updated_at")
      .HasColumnType("datetime2")
      .HasDefaultValueSql("SYSUTCDATETIME()")
      .IsRequired();

    // Check constraint: keep balances non-negative
    builder.ToTable(t => t.HasCheckConstraint("ck_vote_accounts_balance_nonneg", "[balance] >= 0"));
  }
}
