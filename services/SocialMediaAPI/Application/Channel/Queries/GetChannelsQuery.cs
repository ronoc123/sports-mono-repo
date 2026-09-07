using Application.Channel.Dto;
using Contracts.Contracts;
using Domain.Channel;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Queries;

public record GetChannelsQuery : IRequest<ServiceResponse<List<ChannelSummaryResponse>>>;

public class GetChannelsQueryHandler : IRequestHandler<GetChannelsQuery, ServiceResponse<List<ChannelSummaryResponse>>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public GetChannelsQueryHandler(IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<List<ChannelSummaryResponse>>> Handle(
        GetChannelsQuery request,
        CancellationToken cancellationToken)
    {
        var channels = await _channelRepository.GetAllAsync(cancellationToken);

        var summaries = channels.Select(c => new ChannelSummaryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            LinkedAccountCount = c.LinkedAccounts.Count,
            CreatedAt = c.CreatedAt
        }).ToList();

        return ServiceResponse.Ok(summaries);
    }
}
