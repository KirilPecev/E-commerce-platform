using MediatR;

namespace PaymentService.Application.Payments.Command
{
    public record RefundPaymentCommand(
        Guid PaymentId
        ) : IRequest;
}
