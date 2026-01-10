namespace PaymentService.Application.Models
{
    public record PaymentResult(
        bool IsSuccessful,
        string? FailureReason = null
        );
}
