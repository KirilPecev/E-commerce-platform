using MediatR;

namespace PaymentService.Application.Command
{
    public record PayPaymentCommand(
        Guid PaymentId
        ) : IRequest;
}
