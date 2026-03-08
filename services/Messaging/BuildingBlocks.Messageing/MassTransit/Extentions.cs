

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

                var host = configuration["MessageBroker:Host"];
                if (string.IsNullOrWhiteSpace(host))
                {
                    config.UsingInMemory((context, configurator) =>
                    {
                        configurator.ConfigureEndpoints(context);
                    });
                }
                else
                {
                    config.UsingRabbitMq((context, configurator) =>
                    {
                        configurator.Host(host, h =>
                        {
                            h.Username(configuration["MessageBroker:UserName"]);
                            h.Password(configuration["MessageBroker:Password"]);
                        });
                        configurator.ConfigureEndpoints(context);
                    });
                }
            });
            services.AddScoped<IEventPublisher, EventPublisher>();

            return services;
        }
    }
}
