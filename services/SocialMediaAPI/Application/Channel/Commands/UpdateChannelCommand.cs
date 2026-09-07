using Application.Channel.Dto;
using BuildingBlocks.Exceptions;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record UpdateChannelCommand(
    string Id,
    string Name,
    string Description,
    string StyleToneContext
) : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class UpdateChannelCommandValidator : AbstractValidator<UpdateChannelCommand>
{
    public UpdateChannelCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Channel ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Channel name is required.")
            .MaximumLength(100).WithMessage("Channel name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.StyleToneContext)
            .MaximumLength(1000).WithMessage("Style/tone context must not exceed 1000 characters.");
    }
}

public class UpdateChannelCommandHandler : IRequestHandler<UpdateChannelCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public UpdateChannelCommandHandler(IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        UpdateChannelCommand request,
        CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(global::Domain.Channel.Channel), request.Id);

        channel.Name = request.Name;
        channel.Description = request.Description;
        channel.StyleToneContext = request.StyleToneContext;
        channel.LastModified = DateTime.UtcNow;

        await _channelRepository.UpdateAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
