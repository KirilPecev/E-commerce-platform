namespace ECommercePlatform.Application
{
    public class ApplicationSettings
    {
        public string Secret { get; private set; }

        public int TokenExpirationHours { get; private set; }

        public string Audience { get; private set; }

        public string Issuer { get; private set; }

        public string Authority { get; private set; }

        public ApplicationSettings()
        {
            Secret = default!;
            Audience = default!;
            Authority = default!;
            Issuer = default!;
        }
    }
}
