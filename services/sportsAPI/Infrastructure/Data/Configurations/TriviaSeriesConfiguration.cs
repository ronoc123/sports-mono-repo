using Domain.Trivia;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class TriviaSeriesConfiguration : IEntityTypeConfiguration<TriviaSeries>
{
    public void Configure(EntityTypeBuilder<TriviaSeries> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id).HasConversion(
            id => id.Value,
            value => TriviaSeriesId.Of(value));

        builder.Property(s => s.OrganizationId).HasConversion(
            id => id.Value,
            value => OrganizationId.Of(value));

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => s.OrganizationId);

        builder.HasMany(s => s.Questions)
            .WithOne()
            .HasForeignKey(q => q.SeriesId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
