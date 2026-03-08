using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace InventoryService.Tests
{
    public static class InventoryTestTokenGenerator
    {
        private const string Secret = "ultra-super-hard-jwt-ecommerce-secret";
        private const string Audience = "ecommerce-api";
        private const string Issuer = "http://localhost:5001";

        public static string GenerateToken(Guid userId, string email, params string[] roles)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            byte[] key = Encoding.ASCII.GetBytes(Secret);

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("id", userId.ToString()),
                    new Claim(ClaimTypes.Name, email),
                }),
                Audience = Audience,
                Issuer = Issuer,
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            foreach (var role in roles)
            {
                tokenDescriptor.Subject.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public static string GenerateAdminToken()
            => GenerateToken(Guid.NewGuid(), "admin@test.com", "Admin");

        public static string GenerateCustomerToken()
            => GenerateToken(Guid.NewGuid(), "customer@test.com", "Customer");
    }
}
