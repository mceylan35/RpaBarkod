using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class BarcodeService : IBarcodeService
    {
        private readonly IBarcodeRepository _barcodeRepository;

        public BarcodeService(IBarcodeRepository barcodeRepository)
        {
            _barcodeRepository = barcodeRepository;
        }

        public async Task<Barcode> GetAvailableBarcodeAsync()
        {
            return await _barcodeRepository.GetAvailableBarcodeAsync();
        }

        public async Task MarkBarcodeAsUsedAsync(string code, string storeId)
        {
            await _barcodeRepository.MarkAsUsedAsync(code, storeId);
        }

        public async Task<IEnumerable<Barcode>> GetAvailableBarcodesAsync(int count)
        {
            return await _barcodeRepository.GetAvailableBarcodesAsync(count);
        }

        public async Task SeedBarcodesAsync(int count)
        {
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                // Generate EAN-13 barcode
                var barcode = GenerateEan13Barcode(random);
                
                var barcodeEntity = new Barcode
                {
                    Code = barcode,
                    IsUsed = false
                };
                
                await _barcodeRepository.CreateAsync(barcodeEntity);
            }
        }

        private string GenerateEan13Barcode(Random random)
        {
            // Generate 12 random digits
            var digits = new int[12];
            for (int i = 0; i < 12; i++)
            {
                digits[i] = random.Next(0, 10);
            }

            // Calculate check digit
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                sum += digits[i] * (i % 2 == 0 ? 1 : 3);
            }
            int checkDigit = (10 - (sum % 10)) % 10;

            // Combine all digits
            var barcode = string.Join("", digits) + checkDigit;
            return barcode;
        }
    }
}