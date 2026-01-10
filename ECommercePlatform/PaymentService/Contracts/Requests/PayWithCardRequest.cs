namespace PaymentService.Contracts.Requests
{
    public record PayWithCardRequest(
        Guid PaymentId,
        string CardNumber,
        string CardHolder,
        string Expiry,
        string Cvv);
}