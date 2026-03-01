using Application.Common.Interfaces;
using Contracts.Contracts;
using Domain.Product;
using Domain.Repositories;
using Domain.ValueObjects;
using MediatR;
using PurchaseAggregate = Domain.Purchase.Purchase;

namespace Application.Purchase.Commands.CreatePurchaseCommand
{
    public class CreatePurchaseCommandHandler
        : IRequestHandler<CreatePurchaseCommand, ServiceResponse<CreatePurchaseResponse>>
    {
        private readonly IRepository _repo;
        private readonly IPaymentProvider _payment;

        public CreatePurchaseCommandHandler(IRepository repo, IPaymentProvider payment)
        {
            _repo = repo;
            _payment = payment;
        }

        public async Task<ServiceResponse<CreatePurchaseResponse>> Handle(
            CreatePurchaseCommand request,
            CancellationToken cancellationToken)
        {
            var product = await _repo.GetByIdAsync<Product, Guid>(request.ProductId, cancellationToken);
            if (product is null || !product.IsActive)
                return ServiceResponse.Fail<CreatePurchaseResponse>("Product not found or unavailable.");

            var purchaseId = Guid.NewGuid();
            var item = new PurchaseItem(product.Type.ToString(), product.Quantity);
            var purchase = PurchaseAggregate.Create(purchaseId, request.UserId, request.OrgId, item, product.Price);

            await _repo.AddAsync(purchase, cancellationToken);
            await _repo.SaveChangesAsync(cancellationToken);

            var session = await _payment.CreatePaymentIntentAsync(
                purchaseId,
                $"{product.Quantity} votes – {product.Name}",
                product.Price,
                cancellationToken);

            purchase.AttachStripeSession(ExternalSessionId.Create(session.PaymentIntentId));
            await _repo.SaveChangesAsync(cancellationToken);

            return ServiceResponse.Ok(
                new CreatePurchaseResponse(purchaseId, session.ClientSecret),
                "Payment intent created.");
        }
    }
}
