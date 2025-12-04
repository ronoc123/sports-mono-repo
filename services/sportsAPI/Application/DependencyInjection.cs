using Application.Common.Behaviours;
using Application.Profiles;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // Add MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Add pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        // Add FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        // Add AutoMapper

        services.AddAutoMapper(typeof(OrganizationMappingPorfile).Assembly);
        services.AddAutoMapper(typeof(PlayerOptionMappingProfile).Assembly);
        services.AddAutoMapper(typeof(PlayerMappingProfile).Assembly);
        services.AddAutoMapper(typeof(LeagueMappingProfile).Assembly);


    return services;
    }
}
