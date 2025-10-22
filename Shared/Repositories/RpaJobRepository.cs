using MongoDB.Driver;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Repositories
{
    public class RpaJobRepository : IRpaJobRepository
    {
        private readonly MongoDbContext _context;

        public RpaJobRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<RpaJob> CreateAsync(RpaJob job)
        {
            // Set expiration time (e.g., 1 hour from now)
            job.ExpiresAt = DateTime.UtcNow.AddHours(1);
            await _context.RpaJobs.InsertOneAsync(job);
            return job;
        }

        public async Task<RpaJob> GetByIdAsync(string id)
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.Id, id);
            return await _context.RpaJobs.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<RpaJob>> GetPendingJobsAsync(int limit)
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.Status, JobStatus.Pending);
            var sort = Builders<RpaJob>.Sort.Ascending(j => j.CreatedAt);
            return await _context.RpaJobs.Find(filter).Sort(sort).Limit(limit).ToListAsync();
        }

        public async Task<IEnumerable<RpaJob>> GetJobsByStoreIdAsync(string storeId)
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.StoreId, storeId);
            return await _context.RpaJobs.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<RpaJob>> GetJobsByClientIdAsync(string clientId)
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.ClientId, clientId);
            return await _context.RpaJobs.Find(filter).ToListAsync();
        }

        public async Task<IEnumerable<RpaJob>> GetExpiredJobsAsync()
        {
            var filter = Builders<RpaJob>.Filter.And(
                Builders<RpaJob>.Filter.Eq(j => j.Status, JobStatus.InProgress),
                Builders<RpaJob>.Filter.Lt(j => j.ExpiresAt, DateTime.UtcNow)
            );
            return await _context.RpaJobs.Find(filter).ToListAsync();
        }

        public async Task UpdateAsync(RpaJob job)
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.Id, job.Id);
            await _context.RpaJobs.ReplaceOneAsync(filter, job);
        }

        public async Task AssignJobToClientAsync(string jobId, string clientId)
        {
            var filter = Builders<RpaJob>.Filter.And(
                Builders<RpaJob>.Filter.Eq(j => j.Id, jobId),
                Builders<RpaJob>.Filter.Eq(j => j.Status, JobStatus.Pending)
            );
            
            var update = Builders<RpaJob>.Update
                .Set(j => j.ClientId, clientId)
                .Set(j => j.Status, JobStatus.InProgress)
                .Set(j => j.StartedAt, DateTime.UtcNow)
                .Set(j => j.UpdatedAt, DateTime.UtcNow);
            
            await _context.RpaJobs.UpdateOneAsync(filter, update);
        }

        public async Task<IEnumerable<RpaJob>> GetInProgressJobsAsync()
        {
            var filter = Builders<RpaJob>.Filter.Eq(j => j.Status, JobStatus.InProgress);
            return await _context.RpaJobs.Find(filter).ToListAsync();
        }
    }
}