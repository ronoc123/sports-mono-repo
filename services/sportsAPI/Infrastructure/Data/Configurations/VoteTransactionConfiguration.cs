using System;
using Domain.Shared_kernel;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class VoteTransactionConfiguration : IEntityTypeConfiguration<VoteTransaction>
{
  public void Configure(EntityTypeBuilder<VoteTransaction> builder)
  {
    builder.HasKey(x => x.Id);

    builder.Property(c => c.LeagueId).HasConversion(
      leagueId => leagueId.Value,
      value => LeagueId.Of(value));

    builder.Property(c => c.UserId).HasConversion(
      userId => userId.Value,
      value => UserId.Of(value));

    builder.Property(p => p.PlayerOptionId)
        .HasConversion(
            v => v == null ? (Guid?)null : v.Value,
            v => v.HasValue ? PlayerOptionId.Of(v.Value) : null);

    builder.Property(x => x.CreatedAt)
        .HasColumnType("datetimeoffset");
  }
}
