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

    // If your AR base exposes Id: VoteAccountId, ignore it for EF
    builder.Ignore(x => x.Id);

    // Composite PK on scalars (org_id, user_id)
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

    builder.HasIndex(x => new { x.OrgId, x.UserId })
           .HasDatabaseName("ix_vote_accounts_org_user");

    // Keep FK to Organization (same context)
    builder.HasOne<Domain.Organizations.Organization>()
           .WithMany()
           .HasForeignKey("org_id")
           .OnDelete(DeleteBehavior.Restrict);

  }
}
