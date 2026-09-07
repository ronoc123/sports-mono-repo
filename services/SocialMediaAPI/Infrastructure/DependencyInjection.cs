using Application.Common.Interfaces;
using Domain.Channel;
using Domain.PostCycle;
using Domain.Records;
using Domain.VideoGenerationJob;
using Infrastructure.Adapters;
using Infrastructure.Data;
using Infrastructure.OAuth;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Infrastructure.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;
using SportifyCore.Domain;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterBsonClassMaps();

        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<ISocialMediaDbContext, SocialMediaDbContext>();

        // Repositories
        services.AddScoped<IRepository<Channel, string>, ChannelRepository>();
        services.AddScoped<IPostRecordRepository, PostRecordRepository>();
        services.AddScoped<IPostCycleRepository, PostCycleRepository>();
        services.AddScoped<IVideoGenerationJobRepository, VideoGenerationJobRepository>();

        // Encryption
        services.AddSingleton<IEncryptionService, AesEncryptionService>();

        // YouTube OAuth
        services.AddHttpClient<IYouTubeOAuthService, GoogleYouTubeOAuthService>();

        // Video generation adapter — provider-switched via config
        services.Configure<VideoGenerationSettings>(
            configuration.GetSection(VideoGenerationSettings.SectionName));

        var provider = configuration["VideoGeneration:Provider"];
        if (provider == "HiggsFieldClaude")
        {
            services.AddHttpClient<HiggsFieldClaudeAdapter>(client =>
            {
                client.Timeout = TimeSpan.FromMinutes(6); // generation can take 1-3 min
            });
            services.AddScoped<IVideoGenerationAdapter, HiggsFieldClaudeAdapter>();
        }
        else
        {
            services.AddScoped<IVideoGenerationAdapter, StubVideoGenerationAdapter>();
        }

        // Social media adapters (ISocialMediaAdapter, one per platform)
        services.AddHttpClient<YouTubeSocialMediaAdapter>();
        services.AddScoped<ISocialMediaAdapter, YouTubeSocialMediaAdapter>();

        // Post cycle orchestration (Infrastructure — needs IEncryptionService)
        services.AddScoped<IPostCycleOrchestrationService, PostCycleOrchestrationService>();

        // Video generation orchestration
        services.AddScoped<IVideoGenerationOrchestrationService, VideoGenerationOrchestrationService>();

        // Background services
        services.AddHostedService<TempFileCleanupService>();

        return services;
    }

    private static void RegisterBsonClassMaps()
    {
        // Id lives on Entity<string>, not on the derived classes.
        // Register the base class first so MapIdMember targets the correct type.
        if (!BsonClassMap.IsClassMapRegistered(typeof(Entity<string>)))
        {
            BsonClassMap.RegisterClassMap<Entity<string>>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId))
                    .SetIdGenerator(StringObjectIdGenerator.Instance);
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Channel)))
        {
            BsonClassMap.RegisterClassMap<Channel>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PostRecord)))
        {
            BsonClassMap.RegisterClassMap<PostRecord>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(PostCycleJob)))
        {
            BsonClassMap.RegisterClassMap<PostCycleJob>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(VideoGenerationJob)))
        {
            BsonClassMap.RegisterClassMap<VideoGenerationJob>(cm =>
            {
                cm.AutoMap();
                cm.SetIgnoreExtraElements(true);
            });
        }
    }
}
