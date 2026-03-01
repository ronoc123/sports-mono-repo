using Application.Cards.Services;
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
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddValidatorsFromAssembly(assembly);
        services.AddAutoMapper(typeof(OrganizationMappingPorfile).Assembly);
        services.AddAutoMapper(typeof(PlayerOptionMappingProfile).Assembly);
        services.AddAutoMapper(typeof(PlayerMappingProfile).Assembly);
        services.AddAutoMapper(typeof(LeagueMappingProfile).Assembly);
        services.AddScoped<RarityAssignmentService>();
        services.AddScoped<RarityEngine>();
        return services;
    }
}
