using Vega.Models.Database;

namespace Vega.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product?> ViewProductAsync(int id);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId);
        Task<List<Product>> GetProductsBySearchTermAsync(string searchTerm);
        Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice);
        Task<List<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<List<Product>> GetSortedProductsAsync(string sortBy, bool isLowToHigh);
    }
}
