using Domain.Organizations;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id)
            .HasConversion(
                id => id.Value,
                value => OrganizationId.Of(value))
            .IsRequired();

        builder.Property(o => o.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(o => o.TeamId)
            .HasMaxLength(50);

        builder.Property(o => o.TeamName)
            .HasMaxLength(200);

        builder.Property(o => o.TeamShortName)
            .HasMaxLength(10);

        builder.Property(o => o.Sport)
            .HasMaxLength(100);

        builder.Property(o => o.Description)
            .HasMaxLength(1000);

        // Configure LeagueId as foreign key
        builder.Property(o => o.LeagueId)
            .HasConversion(
                id => id.Value,
                value => LeagueId.Of(value))
            .IsRequired();

        // TODO: Configure Value Objects later - temporarily disabled for migration creation
        // The value objects need to be properly configured to avoid EF Core confusion

        // Configure relationships - temporarily commented out to avoid EF Core confusion
        // TODO: Configure relationships properly when navigation properties are set up
        // builder.HasOne<Domain.Leagues.League>()
        //     .WithMany(l => l.Organization)
        //     .HasForeignKey(o => o.LeagueId)
        //     .OnDelete(DeleteBehavior.Restrict);

        // Configure timestamps
        builder.Property(o => o.CreatedAt)
            .IsRequired();
    }
}
