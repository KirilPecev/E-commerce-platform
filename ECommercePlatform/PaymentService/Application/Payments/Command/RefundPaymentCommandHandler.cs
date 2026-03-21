
using MediatR;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Payments.Command
{
    public class RefundPaymentCommandHandler
        (IPaymentDbContext paymentDbContext) : IRequestHandler<RefundPaymentCommand>
    {
        public async Task Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
        {
            Payment? payment = await paymentDbContext.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found.");

            payment.Refund();

            await paymentDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
