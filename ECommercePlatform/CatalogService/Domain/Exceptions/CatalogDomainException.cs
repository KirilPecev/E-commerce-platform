namespace CatalogService.Domain.Exceptions
{
    public class CatalogDomainException : Exception
    {
        public CatalogDomainException(string message) : base(message)
        {
        }
    }
}
