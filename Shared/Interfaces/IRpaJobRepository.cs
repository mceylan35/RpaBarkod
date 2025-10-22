using Shared.Models;

namespace Shared.Interfaces
{
    public interface IRpaJobRepository
    {
        Task<RpaJob> CreateAsync(RpaJob job);
        Task<RpaJob> GetByIdAsync(string id);
        Task<IEnumerable<RpaJob>> GetPendingJobsAsync(int limit);
        Task<IEnumerable<RpaJob>> GetJobsByStoreIdAsync(string storeId);
        Task<IEnumerable<RpaJob>> GetJobsByClientIdAsync(string clientId);
        Task<IEnumerable<RpaJob>> GetExpiredJobsAsync();
        Task UpdateAsync(RpaJob job);
        Task AssignJobToClientAsync(string jobId, string clientId);
        Task<IEnumerable<RpaJob>> GetInProgressJobsAsync();
    }
}