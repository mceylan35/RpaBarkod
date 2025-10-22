using MongoDB.Driver;
using Shared.Models;

namespace Shared.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(string connectionString, string databaseName)
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
            
            // Create indexes
            CreateIndexes();
        }

        private void CreateIndexes()
        {
            try
            {
                // Create indexes for better performance
                var productBuilder = Builders<Product>.IndexKeys;
                var productIndexModel = new CreateIndexModel<Product>(productBuilder.Ascending(p => p.StoreId));
                Products.Indexes.CreateOne(productIndexModel);
                
                var barcodeBuilder = Builders<Barcode>.IndexKeys;
                var barcodeIndexModel = new CreateIndexModel<Barcode>(barcodeBuilder.Ascending(b => b.IsUsed));
                Barcodes.Indexes.CreateOne(barcodeIndexModel);
                
                var jobBuilder = Builders<RpaJob>.IndexKeys;
                var jobIndexModel1 = new CreateIndexModel<RpaJob>(jobBuilder.Ascending(j => j.Status));
                var jobIndexModel2 = new CreateIndexModel<RpaJob>(jobBuilder.Ascending(j => j.StoreId));
                var jobIndexModel3 = new CreateIndexModel<RpaJob>(jobBuilder.Ascending(j => j.ClientId));
                RpaJobs.Indexes.CreateOne(jobIndexModel1);
                RpaJobs.Indexes.CreateOne(jobIndexModel2);
                RpaJobs.Indexes.CreateOne(jobIndexModel3);
                
                var clientBuilder = Builders<RpaClient>.IndexKeys;
                var clientIndexModel = new CreateIndexModel<RpaClient>(clientBuilder.Ascending(c => c.ClientId));
                RpaClients.Indexes.CreateOne(clientIndexModel);
            }
            catch (Exception ex)
            {
                // Log the exception but don't fail the application startup
                // Indexes can be created later or may already exist
                System.Diagnostics.Debug.WriteLine($"Failed to create indexes: {ex.Message}");
            }
        }

        public IMongoCollection<Product> Products => _database.GetCollection<Product>("products");
        public IMongoCollection<Barcode> Barcodes => _database.GetCollection<Barcode>("barcodes");
        public IMongoCollection<RpaJob> RpaJobs => _database.GetCollection<RpaJob>("rpajobs");
        public IMongoCollection<RpaClient> RpaClients => _database.GetCollection<RpaClient>("rpaclients");
    }
}