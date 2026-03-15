using Domain.BacktestIntegrity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class IntegrityAnswerConfiguration : IEntityTypeConfiguration<IntegrityAnswer>
{
    public void Configure(EntityTypeBuilder<IntegrityAnswer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SessionId)
            .HasConversion(id => id.Value, value => BacktestSessionId.From(value));

        builder.Property(x => x.QuestionIndex).IsRequired();
        builder.Property(x => x.Answer).IsRequired();
        builder.Property(x => x.JournaledReason).HasMaxLength(2000);
    }
}
