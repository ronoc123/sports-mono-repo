using Application.Common.Interfaces;
using Application.VideoGeneration.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;

namespace Application.VideoGeneration.Queries;

public record GetVideoGenerationJobQuery(string JobId)
    : IRequest<ServiceResponse<VideoGenerationJobResponse>>;

public class GetVideoGenerationJobQueryHandler
    : IRequestHandler<GetVideoGenerationJobQuery, ServiceResponse<VideoGenerationJobResponse>>
{
    private readonly IVideoGenerationJobRepository _repository;

    public GetVideoGenerationJobQueryHandler(IVideoGenerationJobRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResponse<VideoGenerationJobResponse>> Handle(
        GetVideoGenerationJobQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException("VideoGenerationJob", request.JobId);

        return ServiceResponse.Ok(new VideoGenerationJobResponse
        {
            Id = job.Id,
            ChannelId = job.ChannelId,
            Status = job.Status,
            HiggsFieldModel = job.HiggsFieldModel,
            RenderedPrompt = job.RenderedPrompt,
            ErrorMessage = job.ErrorMessage,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
        });
    }
}
