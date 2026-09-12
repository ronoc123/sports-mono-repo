using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using Contracts.Contracts;
using Domain.RenderJob;
using FluentValidation;
using MediatR;
using MongoDB.Bson;
using SportifyCore.Domain;
using ChannelEntity = Domain.Channel.Channel;

namespace Application.RenderJobs.Commands;

/// <summary>Per-clip data passed from the API layer to the command.</summary>
public class CreateRenderJobClipRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? StartImageKey { get; set; }
    public string? EndImageKey { get; set; }
}

public record CreateRenderJobCommand(
    string ChannelId,
    string Model,
    int DurationSeconds,
    string Resolution,
    string AspectRatio,
    List<CreateRenderJobClipRequest> Clips,
    Dictionary<string, string>? ModelOptions,
    bool UseChannelImage = true
) : IRequest<ServiceResponse<CreateRenderJobResponse>>;

public class CreateRenderJobCommandValidator : AbstractValidator<CreateRenderJobCommand>
{
    public CreateRenderJobCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("ChannelId is required.");
        RuleFor(x => x.Model).NotEmpty().WithMessage("Model is required.");
        RuleFor(x => x.DurationSeconds)
            .InclusiveBetween(1, 120).WithMessage("DurationSeconds must be between 1 and 120.");
        RuleFor(x => x.Resolution).NotEmpty().WithMessage("Resolution is required.");
        RuleFor(x => x.AspectRatio).NotEmpty().WithMessage("AspectRatio is required.");
        RuleFor(x => x.Clips).NotEmpty().WithMessage("At least one clip is required.");
        RuleForEach(x => x.Clips).ChildRules(clip =>
            clip.RuleFor(c => c.Prompt).NotEmpty().WithMessage("Each clip must have a non-empty prompt."));
    }
}

public class CreateRenderJobCommandHandler
    : IRequestHandler<CreateRenderJobCommand, ServiceResponse<CreateRenderJobResponse>>
{
    private readonly IRenderJobRepository _repository;
    private readonly IRepository<ChannelEntity, string> _channels;
    private readonly IR2StorageService _storage;

    public CreateRenderJobCommandHandler(
        IRenderJobRepository repository,
        IRepository<ChannelEntity, string> channels,
        IR2StorageService storage)
    {
        _repository = repository;
        _channels = channels;
        _storage = storage;
    }

    public async Task<ServiceResponse<CreateRenderJobResponse>> Handle(
        CreateRenderJobCommand request,
        CancellationToken cancellationToken)
    {
        var jobId = ObjectId.GenerateNewId().ToString();

        // Upload the channel character image once if UseChannelImage is true and
        // at least one clip does not supply its own start frame.
        string? channelImageKey = null;
        if (request.UseChannelImage && request.Clips.Any(c => string.IsNullOrEmpty(c.StartImageKey)))
        {
            var uploaded = await TryUploadChannelImageAsync(request.ChannelId, jobId, cancellationToken);
            channelImageKey = uploaded.FirstOrDefault();
        }

        var clips = request.Clips.Select(c => new RenderJobClip
        {
            Prompt = c.Prompt,
            StartImageKey = !string.IsNullOrEmpty(c.StartImageKey) ? c.StartImageKey : channelImageKey,
            EndImageKey = string.IsNullOrEmpty(c.EndImageKey) ? null : c.EndImageKey,
        }).ToList();

        var job = new RenderJob
        {
            Id = jobId,
            ChannelId = request.ChannelId,
            Status = "Pending",
            Prompt = clips.FirstOrDefault()?.Prompt ?? string.Empty,
            Model = request.Model,
            DurationSeconds = request.DurationSeconds,
            Resolution = request.Resolution,
            AspectRatio = request.AspectRatio,
            ModelOptions = request.ModelOptions ?? new Dictionary<string, string>(),
            OutputVideoKey = $"generation/{jobId}/output.mp4",
            Clips = clips,
        };

        await _repository.AddAsync(job, cancellationToken);

        return ServiceResponse.Ok(new CreateRenderJobResponse { JobId = job.Id });
    }

    private async Task<List<string>> TryUploadChannelImageAsync(
        string channelId,
        string jobId,
        CancellationToken cancellationToken)
    {
        var channel = await _channels.GetByIdAsync(channelId, cancellationToken);

        if (channel?.CharacterImagePath is null || !File.Exists(channel.CharacterImagePath))
            return new List<string>();

        var ext = Path.GetExtension(channel.CharacterImagePath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".png"  => "image/png",
            ".webp" => "image/webp",
            _       => "image/jpeg",
        };

        var objectKey = $"generation/{jobId}/reference{ext}";

        await using var stream = File.OpenRead(channel.CharacterImagePath);
        await _storage.UploadAsync(objectKey, stream, contentType, cancellationToken);

        return new List<string> { objectKey };
    }
}
