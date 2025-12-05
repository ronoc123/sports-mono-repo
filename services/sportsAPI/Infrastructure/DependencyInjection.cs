using Application;
using Application.Common.Interfaces;
using Domain.Abstractions;
using Domain.DomainServices.RewardService;
using Domain.DomainServices.Voting;
using Domain.DomainServices.VotingService.VotingService;
using Domain.Organizations.Entities;
using Domain.Repositories;
using Domain.ValueObjects.ConcreteTypes;
using Infrastructure.Data;
using Infrastructure.Events;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Infrastructure
{
  public static class DependencyInjection
  {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
      // Register DbContext with a connection string from appsettings.json
      services.AddDbContext<SportsDbAppContext>(options =>
          options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                  sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)));

      // Register IApplicationDbContext
      services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SportsDbAppContext>());

      // TO BE REMOVED 
      services.AddScoped<IOrganizationRepository, OrganizationRepository>();

      services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

      services.AddScoped<IVotingService, VotingService>();
      services.AddScoped<IRewardRedemptionService, RewardRedemptionService>();


      services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

      services.AddScoped<IRepository, Repository>();

      return services;
    }
  }
}
