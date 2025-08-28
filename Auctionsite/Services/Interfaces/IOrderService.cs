using Vega.Models.Database;

namespace Vega.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> GetOrderByIdAsync(int id);
        Task<List<Order>> GetAllOrdersAsync();
        Task<List<Order>> GetOrdersByUserIdAsync(string userId);
        Task<Order> GetOrCreateCartAsync(string userId);
        Task<bool> AddProductToCartAsync(string userId, int productId, int amount);
        Task<bool> RemoveProductFromCartAsync(string userId, int productId);
        Task<bool> IncrementProductAmountAsync(string userId, int productId);
        Task<bool> DecrementProductAmountAsync(string userId, int productId);
        Task<bool> PlaceOrderAsync(string userId, decimal shipping, string? shippingMethod);
        Task<bool> DeleteOrderAsync(int orderid);
    }
}