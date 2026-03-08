using Application;
using Application.Common.Interfaces;
using Application.Common.Services;
using Domain.Abstractions;
using Domain.DomainServices.RewardService;
using Domain.DomainServices.Voting;
using Domain.DomainServices.VotingService.VotingService;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Events;
using Infrastructure.Integrations.Identity;
using Infrastructure.Repositories;
using Infrastructure.Rewards;
using Infrastructure.Store;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Domain.Poll;
using Domain.Trivia;


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

            services.AddScoped<IDomainVotingService, DomainVotingService>();
            services.AddScoped<IVotingService, VotingService>();
            services.AddScoped<IRewardRedemptionService, RewardRedemptionService>();
            services.AddScoped<IQrCodeGenerator, QrCodeGenerator>();
            services.AddScoped<IRedemptionCodeGenerator, RedemptionCodeGenerator>();

            services.AddHttpClient<IUserDirectory, UserDirectoryClient>(client =>
            {
                var baseUrl = configuration["IdentityService:BaseUrl"] ?? "http://identityapi:8080/";
                if (!baseUrl.EndsWith("/")) baseUrl += "/";
                client.BaseAddress = new Uri(baseUrl);
            });


            services.AddMediatR(cfg =>
              cfg.RegisterServicesFromAssembly(typeof(ApplicationAssemblyMarker).Assembly));

            services.AddScoped<IRepository, Repository>();

            // Dashboard Engagement
            services.AddScoped<ITriviaSeriesRepository, TriviaSeriesRepository>();
            services.AddScoped<IPollRepository, PollRepository>();

            services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
            services.AddScoped<IPaymentProvider, StripePaymentProvider>();

            return services;
        }
    }
}
