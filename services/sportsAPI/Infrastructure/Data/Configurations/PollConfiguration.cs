using Domain.Poll;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id).HasConversion(
            id => id.Value,
            value => PollId.Of(value));

        builder.Property(p => p.OrganizationId).HasConversion(
            id => id.Value,
            value => OrganizationId.Of(value));

        builder.Property(p => p.QuestionText).IsRequired().HasMaxLength(1000);
        builder.Property(p => p.Status).IsRequired().HasConversion<int>();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.OrganizationId, p.Status });

        builder.HasMany(p => p.Options)
            .WithOne()
            .HasForeignKey(o => o.PollId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
