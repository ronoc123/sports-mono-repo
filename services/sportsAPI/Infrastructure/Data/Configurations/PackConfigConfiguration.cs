using Domain.Cards;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public sealed class PackConfigConfiguration : IEntityTypeConfiguration<PackConfig>
{
    public void Configure(EntityTypeBuilder<PackConfig> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrgId).IsRequired();
        builder.Property(x => x.PackPointCost).IsRequired();

        builder.HasIndex(x => x.OrgId).IsUnique();
    }
}
