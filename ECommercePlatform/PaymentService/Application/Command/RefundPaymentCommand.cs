using MediatR;

namespace PaymentService.Application.Command
{
    public record RefundPaymentCommand(
        Guid PaymentId
        ) : IRequest;
}
