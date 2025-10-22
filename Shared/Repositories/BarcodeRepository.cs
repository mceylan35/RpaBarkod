using MongoDB.Driver;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Repositories
{
    public class BarcodeRepository : IBarcodeRepository
    {
        private readonly MongoDbContext _context;

        public BarcodeRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Barcode> CreateAsync(Barcode barcode)
        {
            await _context.Barcodes.InsertOneAsync(barcode);
            return barcode;
        }

        public async Task<Barcode> GetByCodeAsync(string code)
        {
            var filter = Builders<Barcode>.Filter.Eq(b => b.Code, code);
            return await _context.Barcodes.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Barcode> GetAvailableBarcodeAsync()
        {
            var filter = Builders<Barcode>.Filter.Eq(b => b.IsUsed, false);
            return await _context.Barcodes.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Barcode>> GetAvailableBarcodesAsync(int count)
        {
            var filter = Builders<Barcode>.Filter.Eq(b => b.IsUsed, false);
            return await _context.Barcodes.Find(filter).Limit(count).ToListAsync();
        }

        public async Task UpdateAsync(Barcode barcode)
        {
            var filter = Builders<Barcode>.Filter.Eq(b => b.Id, barcode.Id);
            await _context.Barcodes.ReplaceOneAsync(filter, barcode);
        }

        public async Task MarkAsUsedAsync(string code, string storeId)
        {
            var filter = Builders<Barcode>.Filter.Eq(b => b.Code, code);
            var update = Builders<Barcode>.Update
                .Set(b => b.IsUsed, true)
                .Set(b => b.UsedByStoreId, storeId)
                .Set(b => b.UsedAt, DateTime.UtcNow)
                .Set(b => b.UpdatedAt, DateTime.UtcNow);
            
            await _context.Barcodes.UpdateOneAsync(filter, update);
        }

        public async Task<IEnumerable<Barcode>> GetUsedBarcodesByStoreAsync(string storeId)
        {
            var filter = Builders<Barcode>.Filter.And(
                Builders<Barcode>.Filter.Eq(b => b.IsUsed, true),
                Builders<Barcode>.Filter.Eq(b => b.UsedByStoreId, storeId)
            );
            return await _context.Barcodes.Find(filter).ToListAsync();
        }
    }
}