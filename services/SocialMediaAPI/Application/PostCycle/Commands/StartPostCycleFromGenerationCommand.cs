using Application.Common.Interfaces;
using Application.PostCycle.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.PostCycle;
using Domain.VideoGenerationJob;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SportifyCore.Domain;

namespace Application.PostCycle.Commands;

public record StartPostCycleFromGenerationCommand(
    string ChannelId,
    string VideoGenerationJobId,
    string Title,
    string Description,
    List<string> Hashtags)
    : IRequest<ServiceResponse<StartPostCycleResponse>>;

public class StartPostCycleFromGenerationCommandValidator
    : AbstractValidator<StartPostCycleFromGenerationCommand>
{
    public StartPostCycleFromGenerationCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.VideoGenerationJobId).NotEmpty().WithMessage("Video generation job ID is required.");
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required.");
    }
}

public class StartPostCycleFromGenerationCommandHandler
    : IRequestHandler<StartPostCycleFromGenerationCommand, ServiceResponse<StartPostCycleResponse>>
{
    private readonly IVideoGenerationJobRepository _videoGenerationJobRepository;
    private readonly IPostCycleRepository _postCycleRepository;
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public StartPostCycleFromGenerationCommandHandler(
        IVideoGenerationJobRepository videoGenerationJobRepository,
        IPostCycleRepository postCycleRepository,
        IRepository<global::Domain.Channel.Channel, string> channelRepository,
        IServiceScopeFactory scopeFactory)
    {
        _videoGenerationJobRepository = videoGenerationJobRepository;
        _postCycleRepository = postCycleRepository;
        _channelRepository = channelRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<ServiceResponse<StartPostCycleResponse>> Handle(
        StartPostCycleFromGenerationCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        if (channel.LinkedAccounts.Count == 0)
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "No social media accounts are linked to this channel. Connect an account in channel settings before posting.");

        if (channel.LinkedAccounts.All(a => a.TokenStatus == "invalid"))
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "All linked accounts have expired or invalid tokens. Reconnect your accounts in channel settings.");

        var generationJob = await _videoGenerationJobRepository.GetByIdAsync(
            request.VideoGenerationJobId, cancellationToken)
            ?? throw new EntityNotFoundException("VideoGenerationJob", request.VideoGenerationJobId);

        if (generationJob.Status != "Ready")
            return ServiceResponse.Fail<StartPostCycleResponse>(
                $"Video generation job is not ready (status: {generationJob.Status}).");

        if (string.IsNullOrEmpty(generationJob.VideoTempPath))
            return ServiceResponse.Fail<StartPostCycleResponse>(
                "Generated video path is missing.");

        // Mark generation job as consumed so cleanup service skips it
        generationJob.Status = "Consumed";
        await _videoGenerationJobRepository.UpdateAsync(generationJob, cancellationToken);

        var generationMetadata = new GenerationMetadata
        {
            Method = "higgsfield-claude-mcp",
            HiggsFieldModel = generationJob.HiggsFieldModel,
            RenderedPrompt = generationJob.RenderedPrompt,
            ImageReference = generationJob.ImageFileName,
        };

        var job = new PostCycleJob
        {
            ChannelId = request.ChannelId,
            VideoPath = generationJob.VideoTempPath,
            Title = request.Title,
            Description = request.Description,
            Hashtags = request.Hashtags,
            Status = "Running",
            GenerationMetadata = generationMetadata,
        };

        await _postCycleRepository.AddAsync(job, cancellationToken);

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
