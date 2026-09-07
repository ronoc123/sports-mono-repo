using Application.Common.Interfaces;
using Application.PostCycle.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;

namespace Application.PostCycle.Queries;

public record GetPostCycleQuery(string JobId) : IRequest<ServiceResponse<PostCycleJobResponse>>;

public class GetPostCycleQueryHandler
    : IRequestHandler<GetPostCycleQuery, ServiceResponse<PostCycleJobResponse>>
{
    private readonly IPostCycleRepository _postCycleRepository;

    public GetPostCycleQueryHandler(IPostCycleRepository postCycleRepository)
    {
        _postCycleRepository = postCycleRepository;
    }

    public async Task<ServiceResponse<PostCycleJobResponse>> Handle(
        GetPostCycleQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _postCycleRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException("PostCycleJob", request.JobId);

        return ServiceResponse.Ok(new PostCycleJobResponse
        {
            Id = job.Id,
            ChannelId = job.ChannelId,
            Status = job.Status,
            Title = job.Title,
            PlatformJobs = job.PlatformJobs.Select(pj => new PlatformJobResponse
            {
                Platform = pj.Platform,
                Status = pj.Status,
                VideoUrl = pj.VideoUrl,
                ExternalPostId = pj.ExternalPostId,
                ErrorMessage = pj.ErrorMessage,
                RequiresReauth = pj.RequiresReauth,
            }).ToList(),
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
        });
    }
}
