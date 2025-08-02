using Domain.Users;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.Of(value))
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasMaxLength(256)
            .IsRequired();

        // Configure timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        // Configure relationships - temporarily commented out to avoid EF Core confusion
        // TODO: Configure relationships properly when navigation properties are set up
        // builder.HasMany(u => u.Votes)
        //     .WithOne()
        //     .HasForeignKey("UserId")
        //     .OnDelete(DeleteBehavior.Cascade);

        // builder.HasMany(u => u.Codes)
        //     .WithOne()
        //     .HasForeignKey("RedeemerId")
        //     .OnDelete(DeleteBehavior.SetNull);

        // builder.HasMany(u => u.VotesAvailable)
        //     .WithOne()
        //     .HasForeignKey("UserId")
        //     .OnDelete(DeleteBehavior.Cascade);

        // Add unique constraints
        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.UserName)
            .IsUnique();
    }
}
