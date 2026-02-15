using FileImport.Application.Common.Contracts;
using FileImport.Infrastructure.Settings;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace FileImport.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtSettings _jwtSettings;
        public JwtService(JwtSettings jwtSettings)
        {
            _jwtSettings = jwtSettings;
        }
        public string GenerateAccessToken(List<Claim> claims)
        {
            var jsonWebTokenHandler = new JsonWebTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.AccessTokenKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenTTL),
                SigningCredentials = creds
            };
            jsonWebTokenHandler.SetDefaultTimesOnTokenCreation = false;
            var token = jsonWebTokenHandler.CreateToken(tokenDescriptor);
            return token;
        }
        public async Task<int> GetUserIdFromAccessToken(string token)
        {
            var jsonWebTokenHandler = new JsonWebTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.AccessTokenKey)),
                ClockSkew = TimeSpan.Zero
            };
            var tokenValidationResult = await jsonWebTokenHandler.ValidateTokenAsync(token, validationParameters);
            if (!tokenValidationResult.IsValid)
            {
                return 0;
            }
            else
            {
                if (tokenValidationResult.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var userIdObj))
                {
                    int user_id = 0;
                    if (int.TryParse(userIdObj.ToString(), out user_id))
                    {
                        return user_id;
                    }
                    else
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
