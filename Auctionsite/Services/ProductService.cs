using Microsoft.EntityFrameworkCore;
using Vega.Data;
using Vega.Models.Database;
using Vega.Services.Interfaces;

namespace Vega.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _db;
        public ProductService(ApplicationDbContext applicationDbContext )
        {
            _db = applicationDbContext;
        }


        // Get all products including their categories
        public Task<List<Product>> GetAllProductsAsync()
        {
            return _db.Products
                  .Include(p => p.Categories)
                  .ToListAsync();
        }


        // Get a single product by ID including categories
        public Task<Product> GetProductByIdAsync(int id)
        {
            return _db.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        // Another method to view a product (same as GetProductByIdAsync)
        public Task<Product?> ViewProductAsync(int id)
        {
            return _db.Products
                .Include(p => p.Categories)
                .FirstOrDefaultAsync(p => p.Id == id);
        }


        // Add a new product to the database
        public async Task<bool> AddProductAsync(Product product)
        {
            _db.Products.Add(product);
            return await _db.SaveChangesAsync() > 0;
        }


        // Update an existing product
        public async Task<bool> UpdateProductAsync(Product product)
        {
            _db.Products.Update(product);
            return await _db.SaveChangesAsync() > 0;
        }


        // Delete a product by ID
        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return false;

            _db.Products.Remove(product);
            return await _db.SaveChangesAsync() > 0;
        }


        // Get products that belong to a specific category
        public async Task<List<Product>> GetProductsByCategoryIdAsync(int categoryId)
        {
            return await _db.Products
                .Include(p => p.Categories)
                .Where(p => p.Categories.Any(c => c.Id == categoryId))
                .ToListAsync();
        }


        // Get products within a specific price range
        public async Task<List<Product>> GetProductsByPriceRangeAsync(decimal minPrice, decimal maxPrice)
        {
            return await _db.Products
                .Include(p => p.Categories)
                .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
                .ToListAsync();
        }


        // Get products added between two dates
        public async Task<List<Product>> GetProductsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _db.Products
                .Include(p => p.Categories)
                .Where(p => p.AddedAt >= startDate && p.AddedAt <= endDate)
                .ToListAsync();
        }


        // Search products by a search term (title or description)
        public async Task<List<Product>> GetProductsBySearchTermAsync(string searchTerm)
        {
            return await _db.Products
                .Where(p => p.Title.Contains(searchTerm) || p.Description.Contains(searchTerm))
                .Include(p => p.Categories)
                .ToListAsync();
        }


        // Get products sorted by price, title, or date
        public async Task<List<Product>> GetSortedProductsAsync(string sortBy, bool isLowToHigh)
        {
            IQueryable<Product> query = _db.Products.Include(p => p.Categories);

            switch (sortBy.ToLower())
            {
                case "price":
                    query = isLowToHigh ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "title":
                    query = isLowToHigh ? query.OrderBy(p => p.Title) : query.OrderByDescending(p => p.Title);
                    break;
                case "date":
                case "addedat":
                    query = isLowToHigh ? query.OrderBy(p => p.AddedAt) : query.OrderByDescending(p => p.AddedAt);
                    break;
                default:
                    // Default sort by title in ascending order
                    query = query.OrderBy(p => p.Title);
                    break;
            }

            return await query.ToListAsync();
        }


    }
}
