using BuildingBlocks.Messageing.Events;
using MassTransit;
using NotificationAPI.Email;
using NotificationAPI.Email.Templates;

namespace NotificationAPI.Consumers
{
    public class RewardEmailRequestedConsumer : IConsumer<RewardEmailRequestedEvent>
    {
        private readonly IEmailSender _emailSender;

        public RewardEmailRequestedConsumer(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task Consume(ConsumeContext<RewardEmailRequestedEvent> context)
        {
            var message = context.Message;

            var body = RewardEmailTemplate.Render(
                email: message.Email,
                expiresAt: message.RedeemedAt
            );
            await _emailSender.SendAsync(
                 to: message.Email,
                 subject: "You’ve received a reward 🎉",
                 htmlBody: body
            );
        }
    }

}
