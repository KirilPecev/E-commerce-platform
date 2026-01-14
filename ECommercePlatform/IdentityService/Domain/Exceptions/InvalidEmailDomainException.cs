namespace IdentityService.Domain.Exceptions
{
    public class InvalidEmailDomainException : Exception
    {
        public InvalidEmailDomainException(string message) : base(message)
        {
        }
    }
}
