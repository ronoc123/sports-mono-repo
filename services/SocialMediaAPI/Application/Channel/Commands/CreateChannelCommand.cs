using Application.Channel.Dto;
using Contracts.Contracts;
using FluentValidation;
using MediatR;
using SportifyCore.Domain;

namespace Application.Channel.Commands;

public record CreateChannelCommand(
    string Name,
    string Description,
    string StyleToneContext
) : IRequest<ServiceResponse<ChannelDetailResponse>>;

public class CreateChannelCommandValidator : AbstractValidator<CreateChannelCommand>
{
    public CreateChannelCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Channel name is required.")
            .MaximumLength(100).WithMessage("Channel name must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.");

        RuleFor(x => x.StyleToneContext)
            .MaximumLength(1000).WithMessage("Style/tone context must not exceed 1000 characters.");
    }
}

public class CreateChannelCommandHandler : IRequestHandler<CreateChannelCommand, ServiceResponse<ChannelDetailResponse>>
{
    private readonly IRepository<global::Domain.Channel.Channel, string> _channelRepository;

    public CreateChannelCommandHandler(IRepository<global::Domain.Channel.Channel, string> channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<ServiceResponse<ChannelDetailResponse>> Handle(
        CreateChannelCommand request,
        CancellationToken cancellationToken)
    {
        var channel = new global::Domain.Channel.Channel
        {
            Name = request.Name,
            Description = request.Description,
            StyleToneContext = request.StyleToneContext
        };

        await _channelRepository.AddAsync(channel, cancellationToken);

        return ServiceResponse.Ok(ChannelMapper.ToDetailResponse(channel));
    }
}
