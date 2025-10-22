using MongoDB.Driver;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly MongoDbContext _context;

        public ProductRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateAsync(Product product)
        {
            await _context.Products.InsertOneAsync(product);
            return product;
        }

        public async Task<Product> GetByIdAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            return await _context.Products.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Product>> GetByStoreIdAsync(string storeId)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.StoreId, storeId);
            return await _context.Products.Find(filter).ToListAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, product.Id);
            product.UpdatedAt = DateTime.UtcNow;
            await _context.Products.ReplaceOneAsync(filter, product);
        }

        public async Task DeleteAsync(string id)
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, id);
            await _context.Products.DeleteOneAsync(filter);
        }
    }
}