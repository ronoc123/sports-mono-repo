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

public record CreateRenderJobCommand(
    string ChannelId,
    string Prompt,
    string Model,
    int DurationSeconds,
    string Resolution,
    string AspectRatio,
    List<string> ReferenceImageKeys,
    List<string>? KeyframeKeys,
    Dictionary<string, string>? ModelOptions,
    string? ReferenceVideoKey,
    bool UseChannelImage = true
) : IRequest<ServiceResponse<CreateRenderJobResponse>>;

public class CreateRenderJobCommandValidator : AbstractValidator<CreateRenderJobCommand>
{
    public CreateRenderJobCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("ChannelId is required.");
        RuleFor(x => x.Prompt).NotEmpty().WithMessage("Prompt is required.");
        RuleFor(x => x.Model).NotEmpty().WithMessage("Model is required.");
        RuleFor(x => x.DurationSeconds)
            .InclusiveBetween(1, 120).WithMessage("DurationSeconds must be between 1 and 120.");
        RuleFor(x => x.Resolution).NotEmpty().WithMessage("Resolution is required.");
        RuleFor(x => x.AspectRatio).NotEmpty().WithMessage("AspectRatio is required.");
        // ReferenceImageKeys is no longer required from the client —
        // the handler auto-uploads the channel's character image when none are provided.
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

        // Auto-upload the channel's character image when the caller has no explicit
        // reference image keys AND has not opted out via UseChannelImage=false.
        var referenceKeys = request.ReferenceImageKeys.Count > 0
            ? request.ReferenceImageKeys
            : request.UseChannelImage
                ? await TryUploadChannelImageAsync(request.ChannelId, jobId, cancellationToken)
                : new List<string>();

        var job = new RenderJob
        {
            Id = jobId,
            ChannelId = request.ChannelId,
            Status = "Pending",
            Prompt = request.Prompt,
            Model = request.Model,
            DurationSeconds = request.DurationSeconds,
            Resolution = request.Resolution,
            AspectRatio = request.AspectRatio,
            ReferenceImageKeys = referenceKeys,
            KeyframeKeys = request.KeyframeKeys ?? new List<string>(),
            ModelOptions = request.ModelOptions ?? new Dictionary<string, string>(),
            OutputVideoKey = $"generation/{jobId}/output.mp4",
            ReferenceVideoKey = request.ReferenceVideoKey,
        };

        await _repository.AddAsync(job, cancellationToken);

        return ServiceResponse.Ok(new CreateRenderJobResponse { JobId = job.Id });
    }

    private async Task<string?> TryUploadChannelAudioAsync(
        string channelId,
        string jobId,
        CancellationToken cancellationToken)
    {
        var channel = await _channels.GetByIdAsync(channelId, cancellationToken);

        if (channel?.ContextAudioPath is null || !File.Exists(channel.ContextAudioPath))
            return null;

        var ext = Path.GetExtension(channel.ContextAudioPath).ToLowerInvariant();
        var contentType = ext switch
        {
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".aac" => "audio/aac",
            ".m4a" => "audio/mp4",
            ".ogg" => "audio/ogg",
            _      => "audio/mpeg",
        };

        var objectKey = $"generation/{jobId}/audio{ext}";

        await using var stream = File.OpenRead(channel.ContextAudioPath);
        await _storage.UploadAsync(objectKey, stream, contentType, cancellationToken);

        return objectKey;
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
