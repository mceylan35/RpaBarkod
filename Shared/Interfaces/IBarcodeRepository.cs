using Shared.Models;

namespace Shared.Interfaces
{
    public interface IBarcodeRepository
    {
        Task<Barcode> CreateAsync(Barcode barcode);
        Task<Barcode> GetByCodeAsync(string code);
        Task<Barcode> GetAvailableBarcodeAsync();
        Task<IEnumerable<Barcode>> GetAvailableBarcodesAsync(int count);
        Task UpdateAsync(Barcode barcode);
        Task MarkAsUsedAsync(string code, string storeId);
        Task<IEnumerable<Barcode>> GetUsedBarcodesByStoreAsync(string storeId);
    }
}