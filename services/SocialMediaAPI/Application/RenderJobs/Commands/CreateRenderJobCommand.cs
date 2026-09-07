using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using Contracts.Contracts;
using Domain.RenderJob;
using FluentValidation;
using MediatR;
using MongoDB.Bson;

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
        RuleFor(x => x.ReferenceImageKeys)
            .NotEmpty().WithMessage("At least one reference image key is required.");
    }
}

public class CreateRenderJobCommandHandler
    : IRequestHandler<CreateRenderJobCommand, ServiceResponse<CreateRenderJobResponse>>
{
    private readonly IRenderJobRepository _repository;

    public CreateRenderJobCommandHandler(IRenderJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResponse<CreateRenderJobResponse>> Handle(
        CreateRenderJobCommand request,
        CancellationToken cancellationToken)
    {
        var jobId = ObjectId.GenerateNewId().ToString();

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
            ReferenceImageKeys = request.ReferenceImageKeys,
            KeyframeKeys = request.KeyframeKeys ?? new List<string>(),
            ModelOptions = request.ModelOptions ?? new Dictionary<string, string>(),
            OutputVideoKey = $"generation/{jobId}/output.mp4",
        };

        await _repository.AddAsync(job, cancellationToken);

        return ServiceResponse.Ok(new CreateRenderJobResponse { JobId = job.Id });
    }
}
