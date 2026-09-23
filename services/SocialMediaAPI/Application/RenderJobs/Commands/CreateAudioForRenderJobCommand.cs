using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using Contracts.Contracts;
using Domain.RenderJob;
using FluentValidation;
using MediatR;
using MongoDB.Bson;

namespace Application.RenderJobs.Commands;

/// <summary>
/// Creates a new audio-generation job that uses the completed video of a source render job
/// as its input. The VideoWorker will call WanGP's MMAudio or PrismAudio extension on the
/// source video and upload the result (video + AI audio) back to R2.
/// </summary>
public record CreateAudioForRenderJobCommand(
    string SourceJobId,
    string ChannelId,
    /// <summary>"mmaudio" or "prism-audio"</summary>
    string AudioModel,
    string Prompt,
    string NegativePrompt
) : IRequest<ServiceResponse<CreateRenderJobResponse>>;

public class CreateAudioForRenderJobCommandValidator : AbstractValidator<CreateAudioForRenderJobCommand>
{
    public CreateAudioForRenderJobCommandValidator()
    {
        RuleFor(x => x.SourceJobId).NotEmpty().WithMessage("SourceJobId is required.");
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("ChannelId is required.");
        RuleFor(x => x.AudioModel)
            .Must(m => m is "mmaudio" or "prism-audio")
            .WithMessage("AudioModel must be 'mmaudio' or 'prism-audio'.");
        RuleFor(x => x.Prompt).NotEmpty().WithMessage("An audio prompt is required.");
    }
}

public class CreateAudioForRenderJobCommandHandler
    : IRequestHandler<CreateAudioForRenderJobCommand, ServiceResponse<CreateRenderJobResponse>>
{
    private readonly IRenderJobRepository _repository;

    public CreateAudioForRenderJobCommandHandler(IRenderJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResponse<CreateRenderJobResponse>> Handle(
        CreateAudioForRenderJobCommand request,
        CancellationToken cancellationToken)
    {
        var sourceJob = await _repository.GetByIdAsync(request.SourceJobId, cancellationToken);

        if (sourceJob is null)
            return ServiceResponse.Fail<CreateRenderJobResponse>("Source render job not found.");

        if (sourceJob.Status != "Completed")
            return ServiceResponse.Fail<CreateRenderJobResponse>(
                "Audio generation requires a completed video job as the source.");

        if (string.IsNullOrEmpty(sourceJob.OutputVideoKey))
            return ServiceResponse.Fail<CreateRenderJobResponse>(
                "Source job has no output video key.");

        var jobId = ObjectId.GenerateNewId().ToString();

        var modelOptions = new Dictionary<string, string>
        {
            ["negative_prompt"] = request.NegativePrompt,
        };

        var audioJob = new RenderJob
        {
            Id               = jobId,
            ChannelId        = request.ChannelId,
            Status           = "Pending",
            Prompt           = request.Prompt,
            Model            = request.AudioModel,
            DurationSeconds  = sourceJob.DurationSeconds,
            Resolution       = sourceJob.Resolution,
            AspectRatio      = sourceJob.AspectRatio,
            ModelOptions     = modelOptions,
            ReferenceVideoKey = sourceJob.OutputVideoKey,
            OutputVideoKey   = $"generation/{jobId}/output.mp4",
            Clips            = new List<RenderJobClip>(),
        };

        await _repository.AddAsync(audioJob, cancellationToken);

        return ServiceResponse.Ok(new CreateRenderJobResponse { JobId = audioJob.Id });
    }
}
