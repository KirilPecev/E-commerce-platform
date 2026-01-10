using MediatR;

namespace PaymentService.Application.Command
{
    public record PayWithCardCommand(
        Guid PaymentId,
        string CardNumber,
        string CardHolder,
        string Expiry,
        string Cvv
        ) : IRequest;
}
