using System;
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

    // PK
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .HasColumnName("id")
      .ValueGeneratedOnAdd();

    // Columns
    builder.Property(x => x.OrgId)
      .HasColumnName("org_id")
      .HasConversion(v => v.Value, v => OrganizationId.Of(v))
      .IsRequired();

    builder.Property(x => x.UserId)
      .HasColumnName("user_id")
      .HasConversion(v => v.Value, v => UserId.Of(v))
      .IsRequired();


    builder.Property(x => x.CreatedAt)
      .HasColumnName("created_at")
      .HasColumnType("datetimeoffset")
      .HasDefaultValueSql("SYSUTCDATETIME()")
      .IsRequired();

    // Optional vote-specific columns
    builder.Property(x => x.PlayerOptionId)
      .HasColumnName("player_option_id")
      .HasConversion(
        v => v == null ? (Guid?)null : v.Value,
        g => g.HasValue ? PlayerOptionId.Of(g.Value) : (PlayerOptionId?)null);

    builder.Property(x => x.SpendId)
      .HasColumnName("spend_id")
      .HasMaxLength(64);

    builder.Property(x => x.Choice)
      .HasColumnName("choice");

    // Check constraints to enforce invariants:
    // 1) Non-zero amount
    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_amount_nonzero",
      "[amount] <> 0"));

    // 2) Enforce direction by reason (+credit, -spend)
    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_reason_amount_sign",
      "([reason] = 'vote_spend' AND [amount] < 0) OR ([reason] <> 'vote_spend' AND [amount] > 0)"));

    // 3) If it's a vote spend, require player_option_id and spend_id
    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_vote_spend_requires_fields",
      "([reason] <> 'vote_spend') OR ([player_option_id] IS NOT NULL AND [spend_id] IS NOT NULL)"));
  }
}
