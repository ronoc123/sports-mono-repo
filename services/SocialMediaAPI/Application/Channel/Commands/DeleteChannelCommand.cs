using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record DeleteChannelCommand(string Id) : IRequest<ServiceResponse<bool>>;

public class DeleteChannelCommandHandler : IRequestHandler<DeleteChannelCommand, ServiceResponse<bool>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public DeleteChannelCommandHandler(IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<bool>> Handle(
        DeleteChannelCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.Id);

        await _channelRepository.DeleteAsync(channel.Id, cancellationToken);

        return ServiceResponse.Ok(true);
    }
}
