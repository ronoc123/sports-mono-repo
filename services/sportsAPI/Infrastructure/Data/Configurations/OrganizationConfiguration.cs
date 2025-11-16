using Domain.Organizations;
using Domain.ValueObjects;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
  public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
  {
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
      builder.ToTable("organizations");

      // Key
      builder.HasKey(o => o.Id);

      builder.Property(c => c.Id).HasConversion(
        organizationId => organizationId.Value,
        value => OrganizationId.Of(value));

      builder.OwnsOne(o => o.Venue, v =>
      {
        v.Property(p => p.Stadium).HasMaxLength(300);
        v.Property(p => p.Location).HasMaxLength(300);
        v.Property(p => p.Capacity).HasMaxLength(300);
      });

      builder.OwnsOne(o => o.MediaAssets, ma =>
      {
        ma.Property(p => p.BadgeUrl).HasMaxLength(255);
        ma.Property(p => p.LogoUrl).HasMaxLength(255);
        ma.Property(p => p.Fanart1Url).HasMaxLength(255);
        ma.Property(p => p.Fanart2Url).HasMaxLength(255);
        ma.Property(p => p.Fanart3Url).HasMaxLength(255);
        ma.Property(p => p.Fanart4Url).HasMaxLength(255);
        ma.Property(p => p.BannerUrl).HasMaxLength(255);
        ma.Property(p => p.EquipmentUrl).HasMaxLength(255);
      });

      builder.OwnsOne(o => o.SocialLinks, sl =>
      {
        sl.Property(p => p.Website).HasMaxLength(300);
        sl.Property(p => p.Facebook).HasMaxLength(300);
        sl.Property(p => p.Twitter).HasMaxLength(300);
        sl.Property(p => p.Instagram).HasMaxLength(300);
        sl.Property(p => p.YoutubeUrl).HasMaxLength(300);
      });

      builder.OwnsOne(o => o.TeamColors, tc =>
      {
        tc.Property(p => p.Primary).HasMaxLength(16);
        tc.Property(p => p.Secondary).HasMaxLength(16);
        tc.Property(p => p.Tertiary).HasMaxLength(16);
      });

    }
  }
}
