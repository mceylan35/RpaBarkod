using Shared.Models;

namespace Shared.Interfaces
{
    public interface IBarcodeService
    {
        Task<Barcode> GetAvailableBarcodeAsync();
        Task MarkBarcodeAsUsedAsync(string code, string storeId);
        Task SeedBarcodesAsync(int count);
        Task<IEnumerable<Barcode>> GetAvailableBarcodesAsync(int count);
    }
}