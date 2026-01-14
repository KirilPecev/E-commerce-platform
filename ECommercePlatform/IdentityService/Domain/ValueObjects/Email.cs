using IdentityService.Domain.Exceptions;

namespace IdentityService.Domain.ValueObjects
{
    public record Email
    {
        public string Value { get; }

        public Email(string value)
        {
            if (!value.Contains("@"))
                throw new InvalidEmailDomainException(value);

            Value = value;
        }

        public override string ToString() => Value;
    }
}
