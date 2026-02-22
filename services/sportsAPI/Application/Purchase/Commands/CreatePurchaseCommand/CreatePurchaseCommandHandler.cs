using Contracts.Contracts;
using Domain.Repositories;
using MediatR;

namespace Application.Purchase.Commands.CreatePurchaseCommand
{
    public class CreatePurchaseCommandHandler : IRequestHandler<CreatePurchaseCommand, ServiceResponse<Guid>>
    {
        private readonly IRepository _repo;

        public CreatePurchaseCommandHandler(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<ServiceResponse<Guid>> Handle(CreatePurchaseCommand request, CancellationToken cancellationToken)
        {
            return null;
        }
    }

}
