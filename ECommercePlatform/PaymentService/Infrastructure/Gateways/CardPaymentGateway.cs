
using PaymentService.Application.Interfaces;
using PaymentService.Application.Models;

namespace PaymentService.Infrastructure.Gateways
{
    public class CardPaymentGateway : IPaymentGateway
    {
        public async Task<PaymentResult> ProcessCardPaymentAsync(decimal amount, string currency, CardDetails card, CancellationToken cancellationToken = default)
        {
            // Simulate payment processing delay
            await Task.Delay(1000, cancellationToken);

            // Simple simulation rules
            if (card.CardNumber.StartsWith("4"))
                return new PaymentResult(true);

            return new PaymentResult(
                false,
                "Card declined");
        }
    }
}
