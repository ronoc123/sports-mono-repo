using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Queries;

public record GetChannelQuery(string Id) : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class GetChannelQueryHandler : IRequestHandler<GetChannelQuery, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public GetChannelQueryHandler(IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        GetChannelQuery request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.Id);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
