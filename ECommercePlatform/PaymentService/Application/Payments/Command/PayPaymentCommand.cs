using MediatR;

namespace PaymentService.Application.Payments.Command
{
    public record PayPaymentCommand(
        Guid PaymentId
        ) : IRequest;
}
