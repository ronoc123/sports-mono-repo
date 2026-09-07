using Application.Common.Interfaces;
using Application.PostCycle.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.PostCycle.Commands;

public record RetryPlatformCommand(string JobId, string Platform)
    : IRequest<ServiceResponse<PostCycleJobResponse>>;

public class RetryPlatformCommandHandler
    : IRequestHandler<RetryPlatformCommand, ServiceResponse<PostCycleJobResponse>>
{
    private readonly IPostCycleRepository _postCycleRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public RetryPlatformCommandHandler(
        IPostCycleRepository postCycleRepository,
        IServiceScopeFactory scopeFactory)
    {
        _postCycleRepository = postCycleRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<ServiceResponse<PostCycleJobResponse>> Handle(
        RetryPlatformCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _postCycleRepository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException("PostCycleJob", request.JobId);

        var platformJob = job.PlatformJobs.FirstOrDefault(pj =>
            pj.Platform.Equals(request.Platform, StringComparison.OrdinalIgnoreCase))
            ?? throw new EntityNotFoundException("PlatformJob", request.Platform);

        // Reset this platform for retry
        platformJob.Status = "Pending";
        platformJob.ErrorMessage = null;
        platformJob.VideoUrl = null;
        platformJob.ExternalPostId = null;
        platformJob.RequiresReauth = false;
        job.Status = "Running";
        job.CompletedAt = null;

        await _postCycleRepository.UpdateAsync(job, cancellationToken);

        _ = Task.Run(async () =>
        {
            using var scope = _scopeFactory.CreateScope();
            var orchestrator = scope.ServiceProvider
                .GetRequiredService<IPostCycleOrchestrationService>();
            await orchestrator.RetryPlatformAsync(request.JobId, request.Platform, CancellationToken.None);
        });

        return ServiceResponse.Ok(MapToResponse(job));
    }

    private static PostCycleJobResponse MapToResponse(Domain.PostCycle.PostCycleJob job) => new()
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
    };
}
