using Domain.Leagues;
using Domain.Shared_kernel;
using Domain.ValueObjects.ConcreteTypes;
using Domain.VoteAccount;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class VoteAccountConfiguration : IEntityTypeConfiguration<VoteAccount>
{
  public void Configure(EntityTypeBuilder<VoteAccount> builder)
  {
        builder.Ignore(x => x.Id);

        builder.HasKey(x => new { x.LeagueId, x.UserId });

        builder.Property(c => c.LeagueId).HasConversion(
            x => x.Value,
            v => LeagueId.Of(v));

        builder.Property(c => c.UserId).HasConversion(
            x => x.Value,
            v => UserId.Of(v));

        builder.Property(x => x.Balance).IsRequired();
        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.Property(x => x.CreatedAt).HasColumnType("datetime2");
        builder.Property(x => x.UpdatedAt).HasColumnType("datetime2");

        builder.HasOne<League>()
            .WithMany()
            .HasForeignKey(x => x.LeagueId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Navigation(x => x.Transactions)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();
  }
}
