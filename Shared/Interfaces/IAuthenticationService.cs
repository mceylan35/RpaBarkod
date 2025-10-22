using Shared.DTOs;

namespace Shared.Interfaces
{
    public interface IAuthenticationService
    {
        Task<AuthResponseDto> AuthenticateClientAsync(ClientAuthDto authDto);
        Task<bool> ValidateTokenAsync(string token);
    }
}