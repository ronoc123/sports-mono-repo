using Domain.BacktestIntegrity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class BacktestSessionConfiguration : IEntityTypeConfiguration<BacktestSession>
{
    public void Configure(EntityTypeBuilder<BacktestSession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => BacktestSessionId.From(value));

        builder.Property(x => x.UserId).IsRequired();
        builder.HasIndex(x => x.UserId).IsUnique(); // One per user

        builder.OwnsOne(x => x.Metrics, m =>
        {
            m.Property(p => p.TotalTrades).HasColumnName("TotalTrades");
            m.Property(p => p.WinRate).HasColumnName("WinRate").HasColumnType("decimal(5,4)");
            m.Property(p => p.AvgRR).HasColumnName("AvgRR").HasColumnType("decimal(10,4)");
            m.Property(p => p.Expectancy).HasColumnName("Expectancy").HasColumnType("decimal(10,4)");
        });

        builder.HasMany(x => x.Answers)
            .WithOne()
            .HasForeignKey(a => a.SessionId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
