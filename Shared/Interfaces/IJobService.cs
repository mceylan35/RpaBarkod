using Shared.DTOs;
using Shared.Models;

namespace Shared.Interfaces
{
    public interface IJobService
    {
        Task<RpaJob> CreateJobAsync(JobRequestDto jobRequest);
        Task<IEnumerable<RpaJob>> GetPendingJobsAsync(int limit);
        Task<RpaJob> AssignJobToClientAsync(string jobId, string clientId);
        Task UpdateJobResultAsync(JobResultDto jobResult);
        Task<IEnumerable<RpaJob>> GetExpiredJobsAsync();
        Task<IEnumerable<RpaJob>> GetInProgressJobsAsync();
        Task<RpaJob> GetJobByIdAsync(string jobId);
    }
}