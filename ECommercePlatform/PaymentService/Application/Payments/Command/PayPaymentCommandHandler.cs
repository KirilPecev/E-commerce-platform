
using MediatR;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;
using PaymentService.Domain.Aggregates;

namespace PaymentService.Application.Payments.Command
{
    public class PayPaymentCommandHandler
        (IPaymentDbContext paymentDbContext) : IRequestHandler<PayPaymentCommand>
    {
        public async Task Handle(PayPaymentCommand request, CancellationToken cancellationToken)
        {
            Payment? payment = await paymentDbContext.Payments.FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null) throw new KeyNotFoundException($"Payment with ID {request.PaymentId} not found.");

            payment.MarkAsPaid(payment.PaymentMethod);

            await paymentDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
