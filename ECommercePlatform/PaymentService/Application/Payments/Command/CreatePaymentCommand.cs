using MediatR;

namespace PaymentService.Application.Payments.Command
{
    public record CreatePaymentCommand(
        Guid OrderId,
        decimal Amount,
        string Currency
        ) : IRequest<Guid>;
}
