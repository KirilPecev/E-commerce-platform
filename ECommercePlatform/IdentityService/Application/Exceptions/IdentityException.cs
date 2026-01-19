using Microsoft.AspNetCore.Identity;

namespace IdentityService.Application.Exceptions
{
    public class IdentityException : Exception
    {
        private string message;
        private IEnumerable<IdentityError> errors;

        public IdentityException()
        {
            this.message = "An identity error has occurred.";
            this.errors = Enumerable.Empty<IdentityError>();
        }

        public IdentityException(string message) : base(message)
        {
            this.message = message;
            this.errors = Enumerable.Empty<IdentityError>();
        }

        public IdentityException(string message, IEnumerable<IdentityError> errors)
        {
            this.message = message;
            this.errors = errors;
        }

        public IdentityException(string message, Exception? innerException) : base(message, innerException)
        {
            this.message = message;
            this.errors = Enumerable.Empty<IdentityError>();
        }
    }
}