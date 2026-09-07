using Application.Common.Interfaces;
using Application.PostCycle.Dto;
using Contracts.Contracts;
using Domain.PostCycle;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application.PostCycle.Commands;

public record StartPostCycleCommand(
    string ChannelId,
    string VideoPath,
    string Title,
    string Description,
    List<string> Hashtags)
    : IRequest<ServiceResponse<StartPostCycleResponse>>;

public class StartPostCycleCommandHandler
    : IRequestHandler<StartPostCycleCommand, ServiceResponse<StartPostCycleResponse>>
{
    private readonly IPostCycleRepository _postCycleRepository;
    private readonly IServiceScopeFactory _scopeFactory;

    public StartPostCycleCommandHandler(
        IPostCycleRepository postCycleRepository,
        IServiceScopeFactory scopeFactory)
    {
        _postCycleRepository = postCycleRepository;
        _scopeFactory = scopeFactory;
    }

    public async Task<ServiceResponse<StartPostCycleResponse>> Handle(
        StartPostCycleCommand request,
        CancellationToken cancellationToken)
    {
        var job = new PostCycleJob
        {
            ChannelId = request.ChannelId,
            VideoPath = request.VideoPath,
            Title = request.Title,
            Description = request.Description,
            Hashtags = request.Hashtags,
            Status = "Running",
        };

        await _postCycleRepository.AddAsync(job, cancellationToken);

        // Fire-and-forget background orchestration with its own DI scope
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
