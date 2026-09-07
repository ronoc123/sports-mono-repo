using Application.Common.Interfaces;
using Application.PostRecord.Dto;
using Contracts.Contracts;
using MediatR;

namespace Application.PostRecord.Queries;

public record GetMetadataSuggestionsQuery(string ChannelId)
    : IRequest<ServiceResponse<MetadataSuggestionsResponse>>;

public class GetMetadataSuggestionsQueryHandler
    : IRequestHandler<GetMetadataSuggestionsQuery, ServiceResponse<MetadataSuggestionsResponse>>
{
    private readonly IPostRecordRepository _postRecordRepository;

    public GetMetadataSuggestionsQueryHandler(IPostRecordRepository postRecordRepository)
    {
        _postRecordRepository = postRecordRepository;
    }

    public async Task<ServiceResponse<MetadataSuggestionsResponse>> Handle(
        GetMetadataSuggestionsQuery request,
        CancellationToken cancellationToken)
    {
        var recentRecords = await _postRecordRepository.GetRecentByChannelIdAsync(
            request.ChannelId, 10, cancellationToken);

        var response = new MetadataSuggestionsResponse();

        if (recentRecords.Count > 0)
        {
            var mostRecent = recentRecords[0];
            response.SuggestedTitle = mostRecent.Title;
            response.SuggestedDescription = mostRecent.Description;

            // Top hashtags by frequency across recent records
            response.SuggestedHashtags = recentRecords
                .SelectMany(r => r.Hashtags)
                .GroupBy(h => h, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();
        }

        return ServiceResponse.Ok(response);
    }
}
