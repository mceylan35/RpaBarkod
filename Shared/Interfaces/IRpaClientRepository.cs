using Shared.Models;

namespace Shared.Interfaces
{
    public interface IRpaClientRepository
    {
        Task<RpaClient> CreateAsync(RpaClient client);
        Task<RpaClient> GetByIdAsync(string id);
        Task<RpaClient> GetByClientIdAsync(string clientId);
        Task<IEnumerable<RpaClient>> GetAllAsync();
        Task UpdateAsync(RpaClient client);
        Task<bool> ValidateClientCredentialsAsync(string clientId, string clientSecret);
        Task UpdateLoginStatusAsync(string clientId, bool isLoggedIn, DateTime? expiresAt);
    }
}