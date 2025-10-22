using MongoDB.Driver;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;
using System.Diagnostics;

namespace Shared.Repositories
{
    public class RpaClientRepository : IRpaClientRepository
    {
        private readonly MongoDbContext _context;

        public RpaClientRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<RpaClient> CreateAsync(RpaClient client)
        {
            await _context.RpaClients.InsertOneAsync(client);
            return client;
        }

        public async Task<RpaClient> GetByIdAsync(string id)
        {
            var filter = Builders<RpaClient>.Filter.Eq(c => c.Id, id);
            return await _context.RpaClients.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<RpaClient> GetByClientIdAsync(string clientId)
        {
            Console.WriteLine($"Searching for client with ID: {clientId}");
            var filter = Builders<RpaClient>.Filter.Eq(c => c.ClientId, clientId);
            var client = await _context.RpaClients.Find(filter).FirstOrDefaultAsync();
            Console.WriteLine($"Found client: {client?.Name ?? "null"}");
            return client;
        }

        public async Task<IEnumerable<RpaClient>> GetAllAsync()
        {
            return await _context.RpaClients.Find(_ => true).ToListAsync();
        }

        public async Task UpdateAsync(RpaClient client)
        {
            var filter = Builders<RpaClient>.Filter.Eq(c => c.Id, client.Id);
            client.UpdatedAt = DateTime.UtcNow;
            await _context.RpaClients.ReplaceOneAsync(filter, client);
        }

        public async Task<bool> ValidateClientCredentialsAsync(string clientId, string clientSecret)
        {
            Console.WriteLine($"Validating credentials for client: {clientId}");
            Console.WriteLine($"Client secret provided: {clientSecret}");
            
            var filter = Builders<RpaClient>.Filter.And(
                Builders<RpaClient>.Filter.Eq(c => c.ClientId, clientId),
                Builders<RpaClient>.Filter.Eq(c => c.ClientSecret, clientSecret),
                Builders<RpaClient>.Filter.Eq(c => c.IsActive, true)
            );
            
            // Log the filter for debugging
            Console.WriteLine($"MongoDB filter: clientId={clientId}, clientSecret={clientSecret}, isActive=true");
            var test=   _context.RpaClients.Find(i=>true).ToList();
            var client = await _context.RpaClients.Find(filter).FirstOrDefaultAsync();
            
            Console.WriteLine($"Validation result: {client != null}");
            if (client != null)
            {
                Console.WriteLine($"Client details - Name: {client.Name}, IsActive: {client.IsActive}");
            }
            
            return client != null;
        }

        public async Task UpdateLoginStatusAsync(string clientId, bool isLoggedIn, DateTime? expiresAt)
        {
            var filter = Builders<RpaClient>.Filter.Eq(c => c.ClientId, clientId);
            
            var update = Builders<RpaClient>.Update
                .Set(c => c.LastLoginAt, isLoggedIn ? DateTime.UtcNow : (DateTime?)null)
                .Set(c => c.LoginExpiresAt, expiresAt)
                .Set(c => c.UpdatedAt, DateTime.UtcNow);
            
            await _context.RpaClients.UpdateOneAsync(filter, update);
        }
    }
}