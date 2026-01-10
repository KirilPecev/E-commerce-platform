using MediatR;

namespace PaymentService.Application.Command
{
    public record CreatePaymentCommand(
        Guid OrderId,
        decimal Amount,
        string Currency
        ) : IRequest<Guid>;
}
