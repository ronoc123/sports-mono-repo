using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Application.Common.Interfaces;


namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext with a connection string from appsettings.json
            services.AddDbContext<SportsDbAppContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Register IApplicationDbContext
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<SportsDbAppContext>());

            // Register repositories
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<ILeagueRepository, LeagueRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IVoteRepository, VoteRepository>();

            return services;
        }
    }
}
