
using MediatR;

using PaymentService.Domain.Aggregates;
using PaymentService.Domain.ValueObjects;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Payments.Command
{
    public class CreatePaymentCommandHandler
        (PaymentDbContext paymentDbContext) : IRequestHandler<CreatePaymentCommand, Guid>
    {
        public async Task<Guid> Handle(CreatePaymentCommand request, CancellationToken cancellationToken)
        {
            Payment payment = new Payment(request.OrderId, new Money(request.Amount, request.Currency));

            await paymentDbContext.Payments.AddAsync(payment, cancellationToken);

            await paymentDbContext.SaveChangesAsync(cancellationToken);

            return payment.Id;
        }
    }
}
