using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using APiGamer.Servicios.Abstracciones;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

namespace APiGamer.Servicios
{
    public class TokenServices:ITokenServices
    {
        readonly IConfiguration _configuration;
        public TokenServices(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public string GenerarToken( string Email)
        {
            var claims = new [] {
               new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
               new Claim(JwtRegisteredClaimNames.Sub, Email)
            };
            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_configuration["jwt:ClaveSuperSecretaDeMasDe32caracteres"]!));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["jwt:Issuer"],
                audience: _configuration["jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );
           return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
