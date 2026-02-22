using Contracts.Contracts;
using Domain.Repositories;
using MediatR;

namespace Application.Purchase.Commands.HandleStripeWebhookCommand
{
    public class StripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, ServiceResponse<Guid>>
    {
        private readonly IRepository _repo;

        public StripeWebhookCommandHandler(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResponse<Guid>> Handle(HandleStripeWebhookCommand command, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}
