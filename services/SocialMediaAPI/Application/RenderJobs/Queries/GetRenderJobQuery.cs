using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.RenderJob;
using MediatR;

namespace Application.RenderJobs.Queries;

public record GetRenderJobQuery(string JobId) : IRequest<ServiceResponse<RenderJobResponse>>;

public class GetRenderJobQueryHandler
    : IRequestHandler<GetRenderJobQuery, ServiceResponse<RenderJobResponse>>
{
    private readonly IRenderJobRepository _repository;

    public GetRenderJobQueryHandler(IRenderJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResponse<RenderJobResponse>> Handle(
        GetRenderJobQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RenderJob), request.JobId);

        return ServiceResponse.Ok(MapToResponse(job));
    }

    private static RenderJobResponse MapToResponse(RenderJob job) => new()
    {
        Id = job.Id,
        ChannelId = job.ChannelId,
        Status = job.Status,
        Prompt = job.Prompt,
        Model = job.Model,
        DurationSeconds = job.DurationSeconds,
        Resolution = job.Resolution,
        AspectRatio = job.AspectRatio,
        OutputVideoKey = job.Status == "Completed" ? job.OutputVideoKey : null,
        RetryCount = job.RetryCount,
        ErrorMessage = job.ErrorMessage,
        CreatedAt = job.CreatedAt,
        StartedAt = job.StartedAt,
        CompletedAt = job.CompletedAt,
    };
}
