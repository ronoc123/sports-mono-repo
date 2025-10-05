using Domain.Leagues;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class LeagueConfiguration : IEntityTypeConfiguration<League>
{
    public void Configure(EntityTypeBuilder<League> builder)
    {
        builder.ToTable("Leagues");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasConversion(
                id => id.Value,
                value => LeagueId.Of(value))
            .IsRequired();

        builder.Property(l => l.Name)
            .HasMaxLength(200)
            .IsRequired();

        // Configure timestamps
        builder.Property(l => l.CreatedAt)
            .IsRequired();

        // Configure relationships - temporarily commented out to avoid EF Core confusion
        // TODO: Configure relationships properly when navigation properties are set up
        // builder.HasMany(l => l.Organization)
        //     .WithOne()
        //     .HasForeignKey(o => o.LeagueId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // builder.HasMany(l => l.Players)
        //     .WithOne()
        //     .HasForeignKey(p => p.LeagueId)
        //     .OnDelete(DeleteBehavior.Restrict);
    }
}
