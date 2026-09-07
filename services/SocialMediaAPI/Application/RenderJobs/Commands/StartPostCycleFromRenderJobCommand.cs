using Application.Common.Interfaces;
using Application.PostCycle.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.PostCycle;
using Domain.RenderJob;
using Domain.VideoGenerationJob;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SportifyCore.Domain;

namespace Application.RenderJobs.Commands;

public record StartPostCycleFromRenderJobCommand(
    string RenderJobId,
    string ChannelId,
    string Title,
    string Description,
    List<string> Hashtags)
    : IRequest<ServiceResponse<StartPostCycleResponse>>;

public class StartPostCycleFromRenderJobCommandValidator
    : AbstractValidator<StartPostCycleFromRenderJobCommand>
{
    public StartPostCycleFromRenderJobCommandValidator()
    {
        RuleFor(x => x.RenderJobId).NotEmpty().WithMessage("Render job ID is required.");
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
    }
}

public class StartPostCycleFromRenderJobCommandHandler
    : IRequestHandler<StartPostCycleFromRenderJobCommand, ServiceResponse<StartPostCycleResponse>>
{
    private readonly IRenderJobRepository _renderJobs;
    private readonly IPostCycleRepository _postCycles;
    private readonly IRepository<global::Domain.Channel.Channel, string> _channels;
    private readonly IR2StorageService _storage;
    private readonly IServiceScopeFactory _scopeFactory;

    public StartPostCycleFromRenderJobCommandHandler(
        IRenderJobRepository renderJobs,
        IPostCycleRepository postCycles,
        IRepository<global::Domain.Channel.Channel, string> channels,
        IR2StorageService storage,
        IServiceScopeFactory scopeFactory)
    {
        _renderJobs = renderJobs;
        _postCycles = postCycles;
        _channels = channels;
        _storage = storage;
        _scopeFactory = scopeFactory;
    }

    public async Task<ServiceResponse<StartPostCycleResponse>> Handle(
        StartPostCycleFromRenderJobCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channels.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException("Channel", request.ChannelId);

        if (channel.LinkedAccounts.Count == 0)
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "No social media accounts are linked to this channel. Connect an account in channel settings before posting.");

        if (channel.LinkedAccounts.All(a => a.TokenStatus == "invalid"))
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "All linked accounts have expired or invalid tokens. Reconnect your accounts in channel settings.");

        var renderJob = await _renderJobs.GetByIdAsync(request.RenderJobId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RenderJob), request.RenderJobId);

        if (renderJob.Status != "Completed")
            return ServiceResponse.Fail<StartPostCycleResponse>(
                $"Render job is not completed (status: {renderJob.Status}).");

        if (string.IsNullOrEmpty(renderJob.OutputVideoKey))
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "Render job has no output video.");

        // Download the video from R2 to a local temp file for the post-cycle pipeline
        var tempDir = Path.Combine(Path.GetTempPath(), "render-post-downloads");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{request.RenderJobId}.mp4");

        await using (var r2Stream = await _storage.DownloadAsync(renderJob.OutputVideoKey, cancellationToken))
        await using (var fileStream = File.Create(tempPath))
        {
            await r2Stream.CopyToAsync(fileStream, cancellationToken);
        }

        var job = new PostCycleJob
        {
            ChannelId = request.ChannelId,
            VideoPath = tempPath,
            Title = request.Title,
            Description = request.Description,
            Hashtags = request.Hashtags,
            Status = "Running",
            GenerationMetadata = new GenerationMetadata
            {
                Method = "local-gpu-render",
                RenderedPrompt = renderJob.Prompt,
            },
        };

        await _postCycles.AddAsync(job, cancellationToken);

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var orchestrator = scope.ServiceProvider
                .GetRequiredService<IPostCycleOrchestrationService>();
            await orchestrator.RunAsync(job.Id, CancellationToken.None);
        });

        return ServiceResponse.Ok(new StartPostCycleResponse { JobId = job.Id });
    }
}
