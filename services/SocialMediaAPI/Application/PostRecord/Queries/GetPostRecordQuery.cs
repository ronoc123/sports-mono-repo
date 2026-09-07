using Application.Common.Interfaces;
using Application.PostRecord.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;

namespace Application.PostRecord.Queries;

public record GetPostRecordQuery(string Id) : IRequest<ServiceResponse<PostRecordDetailResponse>>;

public class GetPostRecordQueryHandler
    : IRequestHandler<GetPostRecordQuery, ServiceResponse<PostRecordDetailResponse>>
{
    private readonly IPostRecordRepository _postRecordRepository;

    public GetPostRecordQueryHandler(IPostRecordRepository postRecordRepository)
    {
        _postRecordRepository = postRecordRepository;
    }

    public async Task<ServiceResponse<PostRecordDetailResponse>> Handle(
        GetPostRecordQuery request,
        CancellationToken cancellationToken)
    {
        var record = await _postRecordRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException("PostRecord", request.Id);

        return ServiceResponse.Ok(new PostRecordDetailResponse
        {
            Id = record.Id,
            ChannelId = record.ChannelId,
            Title = record.Title,
            Description = record.Description,
            Hashtags = record.Hashtags,
            VideoReference = record.VideoReference,
            PostedAt = record.CreatedAt,
            PlatformResults = record.PlatformResults.Select(pr => new PlatformResultDetail
            {
                Platform = pr.Platform,
                Status = pr.Status,
                PublishedUrl = pr.PublishedUrl,
                ErrorMessage = pr.ErrorMessage,
                PublishedAt = pr.PublishedAt,
            }).ToList(),
            GenerationMetadata = record.GenerationMetadata is null ? null : new GenerationMetadataResponse
            {
                Method = record.GenerationMetadata.Method,
                HiggsFieldModel = record.GenerationMetadata.HiggsFieldModel,
                RenderedPrompt = record.GenerationMetadata.RenderedPrompt,
                ImageReference = record.GenerationMetadata.ImageReference,
            },
        });
    }
}
