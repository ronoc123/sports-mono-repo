using Domain.H2H;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class H2HMatchConfiguration : IEntityTypeConfiguration<H2HMatch>
{
    public void Configure(EntityTypeBuilder<H2HMatch> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.FanUserId).IsRequired();
        builder.Property(m => m.OrgId).IsRequired();
        builder.Property(m => m.LeagueId).IsRequired();
        builder.Property(m => m.WagerAmount).IsRequired();
        builder.Property(m => m.FanTeamOverall).IsRequired();
        builder.Property(m => m.BotTeamOverall).IsRequired();

        builder.Property(m => m.BotSquadSnapshot)
            .IsRequired()
            .HasMaxLength(4000)
            .HasDefaultValue("[]");

        builder.Property(m => m.Outcome)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pending");

        builder.Property(m => m.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasDefaultValue("pending");

        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.CompletedAt);

        builder.HasIndex(m => new { m.FanUserId, m.OrgId });
        builder.HasIndex(m => m.Status);

        builder.ToTable("H2HMatches");
    }
}
