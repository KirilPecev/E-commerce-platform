using PaymentService.Application.Models;

namespace PaymentService.Application.Interfaces
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessCardPaymentAsync(
            decimal amount,
            string currency,
            CardDetails card,
            CancellationToken cancellationToken = default);
    }
}
