namespace PaymentService.Application.Models
{
    public record CardDetails(
        string CardNumber,
        string CardHolder,
        string Expiry,
        string Cvv);
}
