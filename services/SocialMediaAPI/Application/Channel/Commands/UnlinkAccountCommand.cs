using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record UnlinkAccountCommand(string ChannelId, string Platform)
    : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class UnlinkAccountCommandValidator : AbstractValidator<UnlinkAccountCommand>
{
    public UnlinkAccountCommandValidator()
    {
        RuleFor(x => x.ChannelId).NotEmpty().WithMessage("Channel ID is required.");
        RuleFor(x => x.Platform).NotEmpty().WithMessage("Platform is required.");
    }
}

public class UnlinkAccountCommandHandler
    : IRequestHandler<UnlinkAccountCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public UnlinkAccountCommandHandler(
        IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        UnlinkAccountCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.ChannelId);

        channel.RemoveLinkedAccount(request.Platform);
        channel.LastModified = DateTime.UtcNow;
        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
