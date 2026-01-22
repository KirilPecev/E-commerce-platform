namespace ECommercePlatform.Application
{
    public class ApplicationSettings
    {
        public string Secret { get; private set; }

        public int TokenExpirationHours { get; private set; }

        public ApplicationSettings() => Secret = default!;
    }
}
