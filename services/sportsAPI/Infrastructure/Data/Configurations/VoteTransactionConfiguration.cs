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
    builder.HasKey(x => x.Id);

    builder.Property(x => x.Id)
      .ValueGeneratedOnAdd();

    builder.Property(x => x.OrgId)
      .HasConversion(v => v.Value, v => OrganizationId.Of(v))
      .IsRequired();

    builder.Property(x => x.UserId)
      .HasConversion(v => v.Value, v => UserId.Of(v))
      .IsRequired();

    builder.Property(x => x.CreatedAt)
      .HasColumnType("datetimeoffset")
      .HasDefaultValueSql("SYSUTCDATETIME()")
      .IsRequired();

    builder.Property(p => p.PlayerOptionId)
        .HasConversion(
            v => v == null ? (Guid?)null : v.Value,
            v => v.HasValue ? PlayerOptionId.Of(v.Value) : null);

    builder.Property(x => x.SpendId)
      .HasMaxLength(64);

    builder.Property(x => x.Choice);

    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_amount_nonzero",
      "[amount] <> 0"));

    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_reason_amount_sign",
      "([reason] = 'vote_spend' AND [amount] < 0) OR ([reason] <> 'vote_spend' AND [amount] > 0)"));

    builder.ToTable(t => t.HasCheckConstraint(
      "ck_vt_vote_spend_requires_fields",
      "([reason] <> 'vote_spend') OR ([playerOptionId] IS NOT NULL AND [spendId] IS NOT NULL)"));
  }
}

