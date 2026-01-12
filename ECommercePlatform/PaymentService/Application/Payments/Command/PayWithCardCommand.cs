using MediatR;

namespace PaymentService.Application.Payments.Command
{
    public record PayWithCardCommand(
        Guid PaymentId,
        string CardNumber,
        string CardHolder,
        string Expiry,
        string Cvv
        ) : IRequest;
}
