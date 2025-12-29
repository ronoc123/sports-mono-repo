using Domain.Notification;
using Domain.ValueObjects.ConcreteTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(o => o.Id);

            builder.Property(c => c.Id).HasConversion(
              notificationId => notificationId.Value,
              value => NotificationId.Of(value));

            builder.Property(c => c.OrganizationId).HasConversion(
                organizationId => organizationId.Value,
                value => OrganizationId.Of(value));

            builder.Property(c => c.UserId).HasConversion(
                playerId => playerId.Value,
                value => UserId.Of(value));
        }
    }
}
