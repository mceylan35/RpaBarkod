using Shared.Models;

namespace Shared.Interfaces
{
    public interface IProductRepository
    {
        Task<Product> CreateAsync(Product product);
        Task<Product> GetByIdAsync(string id);
        Task<IEnumerable<Product>> GetByStoreIdAsync(string storeId);
        Task UpdateAsync(Product product);
        Task DeleteAsync(string id);
    }
}