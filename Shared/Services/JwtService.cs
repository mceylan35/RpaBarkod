using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Shared.DTOs;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class JwtService : IAuthenticationService
    {
        private readonly IRpaClientRepository _clientRepository;
        private readonly string _secretKey;
        private readonly int _expiryMinutes;

        public JwtService(IRpaClientRepository clientRepository, string secretKey, int expiryMinutes = 60)
        {
            _clientRepository = clientRepository;
            _secretKey = secretKey;
            _expiryMinutes = expiryMinutes;
        }

        public async Task<AuthResponseDto> AuthenticateClientAsync(ClientAuthDto authDto)
        {
            // Log the incoming request
            Console.WriteLine($"Authenticating client: {authDto.ClientId}");
            
            // Validate input
            if (string.IsNullOrEmpty(authDto.ClientId) || string.IsNullOrEmpty(authDto.ClientSecret))
            {
                throw new ArgumentException("Client ID and Client Secret are required");
            }

            var isValid = await _clientRepository.ValidateClientCredentialsAsync(authDto.ClientId, authDto.ClientSecret);
            
            Console.WriteLine($"Client validation result: {isValid}");
            
            if (!isValid)
            {
                throw new UnauthorizedAccessException("Invalid client credentials");
            }

            var token = GenerateToken(authDto.ClientId);
            
            return new AuthResponseDto
            {
                Token = token,
                ExpiresIn = _expiryMinutes * 60
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_secretKey);
                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string GenerateToken(string clientId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim("clientId", clientId),
                    new Claim("sub", clientId),
                    new Claim("iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                    new Claim("iss", "RaboidRpaSystem"),
                    new Claim("aud", "RpaClients")
                }),
                Expires = DateTime.UtcNow.AddMinutes(_expiryMinutes),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}