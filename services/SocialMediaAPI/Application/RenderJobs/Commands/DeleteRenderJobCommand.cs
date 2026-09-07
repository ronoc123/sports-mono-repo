using Application.Common.Interfaces;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using Domain.RenderJob;
using MediatR;

namespace Application.RenderJobs.Commands;

public record DeleteRenderJobCommand(string JobId) : IRequest<ServiceResponse<bool>>;

public class DeleteRenderJobCommandHandler
    : IRequestHandler<DeleteRenderJobCommand, ServiceResponse<bool>>
{
    private readonly IRenderJobRepository _repository;
    private readonly IR2StorageService _storage;

    public DeleteRenderJobCommandHandler(IRenderJobRepository repository, IR2StorageService storage)
    {
        _repository = repository;
        _storage = storage;
    }

    public async Task<ServiceResponse<bool>> Handle(
        DeleteRenderJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _repository.GetByIdAsync(request.JobId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(RenderJob), request.JobId);

        // Delete R2 assets: output video
        if (!string.IsNullOrEmpty(job.OutputVideoKey))
            await _storage.DeleteAsync(job.OutputVideoKey, cancellationToken);

        // Delete R2 assets: reference images
        foreach (var key in job.ReferenceImageKeys)
            await _storage.DeleteAsync(key, cancellationToken);

        // Delete R2 assets: keyframes
        foreach (var key in job.KeyframeKeys)
            await _storage.DeleteAsync(key, cancellationToken);

        // Remove MongoDB document
        await _repository.DeleteByIdAsync(request.JobId, cancellationToken);

        return ServiceResponse.Ok(true);
    }
}
