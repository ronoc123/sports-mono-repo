using Domain.BiweeklyReview;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class ReviewMessageConfiguration : IEntityTypeConfiguration<ReviewMessage>
{
    public void Configure(EntityTypeBuilder<ReviewMessage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .HasConversion(id => id.Value, value => ReviewSessionId.From(value));

        builder.Property(x => x.Role).IsRequired().HasConversion<string>();
        builder.Property(x => x.Content).IsRequired().HasColumnType("nvarchar(max)");
        builder.Property(x => x.Timestamp).IsRequired();
    }
}
