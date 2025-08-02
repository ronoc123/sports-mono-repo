using Domain.User.Entities;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserVotesConfiguration : IEntityTypeConfiguration<UserVotes>
{
    public void Configure(EntityTypeBuilder<UserVotes> builder)
    {
        builder.ToTable("UserVotes");

        builder.HasKey(uv => uv.Id);

        builder.Property(uv => uv.Id)
            .HasConversion(
                id => id.Value,
                value => UserVotesId.Of(value))
            .IsRequired();

        builder.Property(uv => uv.VotesRemaining)
            .IsRequired();
    }
}
