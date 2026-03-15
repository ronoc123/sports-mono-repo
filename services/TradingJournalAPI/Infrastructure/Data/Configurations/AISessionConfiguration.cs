using Domain.AIConversation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class AISessionConfiguration : IEntityTypeConfiguration<AISession>
{
    public void Configure(EntityTypeBuilder<AISession> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => AISessionId.From(value));

        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.Context).IsRequired().HasConversion<string>();
        builder.Property(x => x.LinkedEntityId).IsRequired();

        builder.HasIndex(x => x.LinkedEntityId);

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(m => m.SessionId)
            .HasPrincipalKey(s => s.Id)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
