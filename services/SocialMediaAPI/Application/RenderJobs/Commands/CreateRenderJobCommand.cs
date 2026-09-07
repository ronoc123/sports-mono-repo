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
    Dictionary<string, string>? ModelOptions
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

        // If the caller did not supply reference image keys, auto-upload the
        // channel's character image (stored locally on the API server) to R2.
        var referenceKeys = request.ReferenceImageKeys.Count > 0
            ? request.ReferenceImageKeys
            : await TryUploadChannelImageAsync(request.ChannelId, jobId, cancellationToken);

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
