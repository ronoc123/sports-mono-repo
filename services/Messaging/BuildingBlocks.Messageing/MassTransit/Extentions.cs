

using BuildingBlocks.Messageing.Publisher;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messageing.MassTransit
{
    public static class Extentions
    {
        public static IServiceCollection AddMessageBroker(this IServiceCollection services, IConfiguration configuration, params Assembly[] assemblies)
        {
            services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();

                if (assemblies.Length > 0)
                {
                    config.AddConsumers(assemblies);
                }

                config.UsingRabbitMq((context, configurator) =>
              {
                  configurator.Host(configuration["MessageBroker:Host"], host =>
                  {
                      host.Username(configuration["MessageBroker:UserName"]);
                      host.Password(configuration["MessageBroker:Password"]);
                  });

                  configurator.ConfigureEndpoints(context);
              });
            });
            services.AddScoped<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}
