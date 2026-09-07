using Application.Common.Interfaces;
using Application.Common.Models;
using Application.VideoGeneration.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.VideoGenerationJob;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SportifyCore.Domain;

namespace Application.VideoGeneration.Commands;

public record StartVideoGenerationCommand(
    string ChannelId,
    string? PromptOverride,
    int TargetDurationSeconds = 15
) : IRequest<ServiceResponse<StartVideoGenerationResponse>>;

public class StartVideoGenerationCommandValidator : AbstractValidator<StartVideoGenerationCommand>
{
    public StartVideoGenerationCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.TargetDurationSeconds)
            .InclusiveBetween(5, 60).WithMessage("Target duration must be between 5 and 60 seconds.");
    }
}

public class StartVideoGenerationCommandHandler
    : IRequestHandler<StartVideoGenerationCommand, ServiceResponse<StartVideoGenerationResponse>>
{
    private const string TempDir = "temp-uploads";

    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IPostRecordRepository _postRecordRepository;
    private readonly IVideoGenerationJobRepository _videoGenerationJobRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public StartVideoGenerationCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IPostRecordRepository postRecordRepository,
        IVideoGenerationJobRepository videoGenerationJobRepository,
        IServiceScopeFactory scopeFactory)
    {
        _channelRepository = channelRepository;
        _postRecordRepository = postRecordRepository;
        _videoGenerationJobRepository = videoGenerationJobRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<ServiceResponse<StartVideoGenerationResponse>> Handle(
        StartVideoGenerationCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        if (string.IsNullOrEmpty(channel.CharacterImagePath) || !File.Exists(channel.CharacterImagePath))
            throw new InvalidOperationException(
                "This channel has no character reference image. Upload one on the channel settings page before generating a video.");

        // Copy the stored character image to a temp file so the cleanup service
        // can safely delete it after generation without affecting the original.
        var ext = Path.GetExtension(channel.CharacterImagePath);
        Directory.CreateDirectory(TempDir);
        var imageTempPath = Path.Combine(TempDir, $"{Guid.NewGuid()}{ext}");
        File.Copy(channel.CharacterImagePath, imageTempPath);

        var recentHistory = await _postRecordRepository.GetRecentByChannelIdAsync(
            request.ChannelId, 5, cancellationToken);

        var historyItems = recentHistory.Select(r => new PostHistoryItem
        {
            Title = r.Title,
            Description = r.Description,
            Hashtags = r.Hashtags,
            PostedAt = r.CreatedAt,
        });

        var renderedPrompt = PromptTemplateRenderer.Render(
            channel.PromptTemplate,
            channel.Name,
            channel.StyleToneContext,
            historyItems,
            request.PromptOverride ?? string.Empty,
            request.TargetDurationSeconds);

        var job = new VideoGenerationJob
        {
            ChannelId = request.ChannelId,
            Status = "Queued",
            ImageTempPath = imageTempPath,
            ImageFileName = Path.GetFileName(channel.CharacterImagePath),
            RenderedPrompt = renderedPrompt,
        };

        await _videoGenerationJobRepository.AddAsync(job, cancellationToken);

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var orchestrator = scope.ServiceProvider
                .GetRequiredService<IVideoGenerationOrchestrationService>();
            await orchestrator.RunAsync(job.Id, CancellationToken.None);
        });

        return ServiceResponse.Ok(new StartVideoGenerationResponse { JobId = job.Id });
    }
}
