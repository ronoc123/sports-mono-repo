using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using Contracts.Contracts;
using Domain.RenderJob;
using MediatR;

namespace Application.RenderJobs.Queries;

public record ListRenderJobsByChannelQuery(string ChannelId)
    : IRequest<ServiceResponse<List<RenderJobResponse>>>;

public class ListRenderJobsByChannelQueryHandler
    : IRequestHandler<ListRenderJobsByChannelQuery, ServiceResponse<List<RenderJobResponse>>>
{
    private readonly IRenderJobRepository _repository;

    public ListRenderJobsByChannelQueryHandler(IRenderJobRepository repository)
        => _repository = repository;

    public async Task<ServiceResponse<List<RenderJobResponse>>> Handle(
        ListRenderJobsByChannelQuery request,
        CancellationToken cancellationToken)
    {
        var jobs = await _repository.ListByChannelAsync(request.ChannelId, cancellationToken);
        return ServiceResponse.Ok(jobs.Select(MapToResponse).ToList());
    }

    internal static RenderJobResponse MapToResponse(RenderJob job) => new()
    {
        Id             = job.Id,
        ChannelId      = job.ChannelId,
        Status         = job.Status,
        Prompt         = job.Prompt,
        Model          = job.Model,
        DurationSeconds = job.DurationSeconds,
        Resolution     = job.Resolution,
        AspectRatio    = job.AspectRatio,
        OutputVideoKey = job.Status == "Completed" ? job.OutputVideoKey : null,
        RetryCount     = job.RetryCount,
        ErrorMessage   = job.ErrorMessage,
        CreatedAt      = job.CreatedAt,
        StartedAt      = job.StartedAt,
        CompletedAt    = job.CompletedAt,
    };
}
