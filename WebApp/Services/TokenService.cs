using Azure.Core.Serialization;
using Messenjoor.Entities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Messenjoor.Shared.Helpers;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Messenjoor.Services
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public static TokenValidationParameters GetTokenValidationParameters(IConfiguration configuration) =>
            new()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = GetSecurityKey(configuration)
            };

        public string GenerateJWT(IEnumerable<Claim>? additionalClaims = null)
        {
            var securityKey = GetSecurityKey(_configuration);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            //var expireInMinutes = Convert.ToInt32(_configuration["Jwt:ExpireMinutes"] ?? "525600");

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);

            //var token = new JwtSecurityToken(issuer: _configuration["Jwt:Issuer"],
            //    audience: "*",
            //  claims: claims,
            //  expires: DateTime.Now.AddMinutes(expireInMinutes),
            //  signingCredentials: credentials);

            //return new JwtSecurityTokenHandler().WriteToken(token);
            var claimsIdentity = new ClaimsIdentity(claims, AuthenticationState.AuthStoreKey);

            // generate token that is valid for 7 days
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Expires = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:ExpireDays"] ?? "365")),
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            //creating a token handler
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            //returning the token back
            return tokenHandler.WriteToken(token);
        }

        public string GenerateJWT(Entities.User user, IEnumerable<Claim>? additionalClaims = null)
        {
            var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                };
            if (additionalClaims?.Any() == true)
                claims.AddRange(additionalClaims);

            return GenerateJWT(claims);
        }

        private static SymmetricSecurityKey GetSecurityKey(IConfiguration _configuration) =>
            new(Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]!));

    }
}
