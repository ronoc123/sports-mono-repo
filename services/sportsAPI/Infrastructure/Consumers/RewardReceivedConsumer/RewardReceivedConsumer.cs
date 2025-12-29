using Application.Notifications.Commands.CreateNotificationCommand;
using BuildingBlocks.Messageing.Events;
using Domain.ValueObjects.ConcreteTypes;
using MassTransit;
using MediatR;

public sealed class RewardReveivedConsumer : IConsumer<RewardEmailRequestedEvent>
{
    private readonly IMediator _mediator;

    public RewardReveivedConsumer(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task Consume(ConsumeContext<RewardEmailRequestedEvent> context)
    {
        var message = context.Message;

        await _mediator.Send(new CreateNotificationCommand(
            UserId.Of(message.UserId),
            OrganizationId.Of(message.OrganizationId),
            RewardItemId.Of(message.RewardId),
            message.Title,
            message.Message
        ));
    }
}
