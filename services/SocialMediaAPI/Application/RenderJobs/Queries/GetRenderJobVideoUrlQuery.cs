using Application.Common.Interfaces;
using Application.RenderJobs.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.RenderJob;
using MediatR;

namespace Application.RenderJobs.Queries;

public record GetRenderJobVideoUrlQuery(string JobId) : IRequest<ServiceResponse<VideoUrlResponse>>;

public class GetRenderJobVideoUrlQueryHandler
    : IRequestHandler<GetRenderJobVideoUrlQuery, ServiceResponse<VideoUrlResponse>>
{
    private readonly IRenderJobRepository _repository;
    private readonly IR2StorageService _storage;

    public GetRenderJobVideoUrlQueryHandler(IRenderJobRepository repository, IR2StorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<ServiceResponse<VideoUrlResponse>> Handle(
        GetRenderJobVideoUrlQuery request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RenderJob), request.JobId);

        if (job.Status != "Completed" || string.IsNullOrEmpty(job.OutputVideoKey))
            return ServiceResponse.Fail<VideoUrlResponse>("Video is not available yet.");

        var url = await _storage.GetPresignedUrlAsync(job.OutputVideoKey, TimeSpan.FromHours(1), cancellationToken);

        return ServiceResponse.Ok(new VideoUrlResponse { Url = url });
    }
}
