using Application.Common.Interfaces;
using Application.PostRecord.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.PostRecord.Queries;

public record GetPostHistoryQuery(string ChannelId, int Page, int PageSize)
    : IRequest<ServiceResponse<PostHistoryPageResponse>>;

public class GetPostHistoryQueryHandler
    : IRequestHandler<GetPostHistoryQuery, ServiceResponse<PostHistoryPageResponse>>
{
    private readonly IPostRecordRepository _postRecordRepository;

    public GetPostHistoryQueryHandler(IPostRecordRepository postRecordRepository)
    {
        _postRecordRepository = postRecordRepository;
    }

    public async Task<ServiceResponse<PostHistoryPageResponse>> Handle(
        GetPostHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var pageSize = Math.Max(1, Math.Min(request.PageSize, 100));
        var page = Math.Max(1, request.Page);

        var (records, totalCount) = await _postRecordRepository.GetPagedByChannelIdAsync(
            request.ChannelId, page, pageSize, cancellationToken);

        var response = new PostHistoryPageResponse
        {
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            Records = records.Select(r => new PostRecordSummaryResponse
            {
                Id = r.Id,
                Title = r.Title,
                DescriptionSnippet = r.Description.Length > 120
                    ? r.Description[..120] + "…"
                    : r.Description,
                VideoReference = r.VideoReference,
                PostedAt = r.CreatedAt,
                PlatformResults = r.PlatformResults.Select(pr => new PlatformResultSummary
                {
                    Platform = pr.Platform,
                    Status = pr.Status,
                }).ToList(),
            }).ToList(),
        };

        return ServiceResponse.Ok(response);
    }
}
