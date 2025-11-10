using Microsoft.IdentityModel.Tokens;
using moduloseguimiento.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace moduloseguimiento.API.Utilities
{
    public class Token
    {

        public static TokenJWT GenerateTokens(ClaimsIdentity claimsIdentity)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Utileria.GetAppSettingsValue("AUTH_JWT:JWT_SECRET_KEY")));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);
            var tokenHandler = new JwtSecurityTokenHandler();
            var accessTokenLifetime = Convert.ToInt32(Utileria.GetAppSettingsValue("AUTH_JWT:JWT_ACCESS_TOKEN_EXPIRE_MINUTES"));
            var refreshTokenLifetime = Convert.ToInt32(Utileria.GetAppSettingsValue("AUTH_JWT:JWT_REFRESH_TOKEN_EXPIRE_MINUTES"));
            var audienceToken = Utileria.GetAppSettingsValue("AUTH_JWT:JWT_AUDIENCE_TOKEN");
            var issuerToken = Utileria.GetAppSettingsValue("AUTH_JWT:JWT_ISSUER_TOKEN");

            var accessToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: claimsIdentity,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(accessTokenLifetime),
                signingCredentials: signingCredentials);

            var refreshTokenClaims = new ClaimsIdentity(claimsIdentity);
            var refreshToken = tokenHandler.CreateJwtSecurityToken(
                audience: audienceToken,
                issuer: issuerToken,
                subject: refreshTokenClaims,
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(refreshTokenLifetime),
                signingCredentials: signingCredentials);

            var token = new TokenJWT
            {
                accessToken = tokenHandler.WriteToken(accessToken),
                refreshToken = tokenHandler.WriteToken(refreshToken)
            };

            return token;
        }


    }
}
