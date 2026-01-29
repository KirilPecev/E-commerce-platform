using System.Text;

using ECommercePlatform.Application;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ECommercePlatform.Identity
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTokenAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            string secret = configuration
                .GetSection(nameof(ApplicationSettings))
                .GetValue<string>(nameof(ApplicationSettings.Secret))!;

            byte[] key = Encoding.ASCII.GetBytes(secret);

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var appSettingsSection = configuration.GetSection(nameof(ApplicationSettings));
                    options.Authority = appSettingsSection.GetValue<string>(nameof(ApplicationSettings.Authority));
                    options.Audience = appSettingsSection.GetValue<string>(nameof(ApplicationSettings.Audience));
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = appSettingsSection.GetValue<string>(nameof(ApplicationSettings.Issuer)),
                        ValidateAudience = true,
                        ValidAudience = appSettingsSection.GetValue<string>(nameof(ApplicationSettings.Audience)),
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            services.AddAuthorization();

            return services;
        }
    }
}
