using Application.Common.Interfaces;
using Domain.AIConversation;
using Domain.BacktestIntegrity;
using Domain.BiweeklyReview;
using Domain.TradeJournal;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TradingJournalDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(5), null)));

        services.AddScoped<IBacktestSessionRepository, BacktestSessionRepository>();
        services.AddScoped<ITradeRepository, TradeRepository>();
        services.AddScoped<IAISessionRepository, AISessionRepository>();
        services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();

        services.AddHttpClient<IAIService, OpenAIService>();

        return services;
    }
}
