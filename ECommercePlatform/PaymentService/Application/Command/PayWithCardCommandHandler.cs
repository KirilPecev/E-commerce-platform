
using MediatR;

using Microsoft.EntityFrameworkCore;

using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;
using PaymentService.Domain.Aggregates;
using PaymentService.Infrastructure.Persistence;

namespace PaymentService.Application.Command
{
    public class PayWithCardCommandHandler
        (PaymentDbContext paymentDbContext,
        IPaymentGateway paymentGateway) : IRequestHandler<PayWithCardCommand>
    {
        public async Task Handle(PayWithCardCommand request, CancellationToken cancellationToken)
        {
            Payment? payment = await paymentDbContext
                .Payments
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId, cancellationToken);

            if (payment == null) throw new KeyNotFoundException($"Payment with {request.PaymentId} not found.");

            PaymentResult result = await paymentGateway.ProcessCardPaymentAsync(
                payment.Amount.Amount,
                payment.Amount.Currency,
                new CardDetails(request.CardNumber, request.CardHolder, request.Expiry, request.Cvv));

            if (result.IsSuccessful)
            {
                payment.MarkAsPaid(PaymentMethod.Card);
            }
            else
            {
                payment.MarkAsFailed(result.FailureReason);
            }

            await paymentDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
