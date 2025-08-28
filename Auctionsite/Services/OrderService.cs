using Vega.Data;
using Vega.Services.Interfaces;
using Vega.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Vega.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;

        public OrderService(ApplicationDbContext db)
        {
            _db = db;
        }


        // Retrieves a specific order with its related products and buyer
        public async Task<Order> GetOrderByIdAsync(int id)
        {
            var order = await _db.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Include(o => o.Buyer)
                .FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }

        // Retrieves all orders that have been placed
        public async Task<List<Order>> GetAllOrdersAsync()
        {
            var orders = await _db.Orders
                .Where(o => o.IsPlaced)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .Include(o => o.Buyer)
                .ToListAsync();
            return orders;
        }

        // Retrieves all placed orders for a specific user
        public async Task<List<Order>> GetOrdersByUserIdAsync(string userId)
        {
            var orders = await _db.Orders
                .Where(o => o.Buyer.Id == userId && o.IsPlaced)
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .ToListAsync();
            return orders;
        }

        // Retrieves the user's shopping cart or creates a new one if it doesn't exist
        public async Task<Order> GetOrCreateCartAsync(string userId)
        {
            var cart = await _db.Orders
                .Include(o => o.OrderProducts)
                .ThenInclude(op => op.Product)
                .FirstOrDefaultAsync(o => o.Buyer.Id == userId && !o.IsPlaced);

            if (cart != null)
                return cart;

            var user = await _db.Users.FindAsync(userId);
            
            // Create a new empty order (cart)
            cart = new Order
            {
                Buyer = user,
                OrderDate = DateTime.Now,
                Shipping = 0,
                TotalPrice = 0,
                IsPlaced = false,
                OrderProducts = new List<OrderProduct>()
            };           

            _db.Orders.Add(cart);
            await _db.SaveChangesAsync();
            return cart;
        }

        // Adds a product to the cart or increases the amount if already in the cart
        public async Task<bool> AddProductToCartAsync(string userId, int productId, int amount)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var product = await _db.Products.FindAsync(productId);
            if (product == null)
            {
                return false;
            }
            
            var existingItem = cart.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (existingItem != null)
            {
                existingItem.Amount += amount;
            }
            else
            {
                cart.OrderProducts.Add(new OrderProduct
                {
                    Product = product,
                    ProductId = productId,
                    Amount = amount,
                    PriceOneCopy = product.Price
                });
            }

            // Update cart total price
            cart.TotalPrice = cart.OrderProducts.Sum(op => op.PriceOneCopy * op.Amount);
            await _db.SaveChangesAsync();
            return true;
        }

        //Removes a product from the cart
        public async Task<bool> RemoveProductFromCartAsync(string userId, int productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if(item == null)
            {
                return false;
            }

            cart.OrderProducts.Remove(item);
            // Update cart total price
            cart.TotalPrice = cart.OrderProducts.Sum(op => op.PriceOneCopy * op.Amount);
            await _db.SaveChangesAsync();
            return true;
        }

        // Increments the amount of a specific product in the cart
        public async Task<bool> IncrementProductAmountAsync(string userId, int productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (item == null)
            {
                return false;
            }

            item.Amount++;
            // Update cart total price
            cart.TotalPrice = cart.OrderProducts.Sum(op => op.PriceOneCopy * op.Amount);
            await _db.SaveChangesAsync();
            return true;
        }

        // Decrements the amount of a product, and removes it if the amount is 0
        public async Task<bool> DecrementProductAmountAsync(string userId, int productId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            var item = cart.OrderProducts.FirstOrDefault(op => op.ProductId == productId);
            if (item == null)
            {
                return false;
            }

            item.Amount--;
            if (item.Amount <= 0)
            {
                cart.OrderProducts.Remove(item);
            }
            // Update cart total price
            cart.TotalPrice = cart.OrderProducts.Sum(op => op.PriceOneCopy * op.Amount);
            await _db.SaveChangesAsync();
            return true;
        }

        // Finalizes the order, marking it as placed
        public async Task<bool> PlaceOrderAsync(string userId, decimal shipping, string? shippingMethod)
        {
            var cart = await GetOrCreateCartAsync(userId);
            if (!cart.OrderProducts.Any())
            {
                return false; // Cart is empty
            }

            cart.IsPlaced = true;
            cart.Shipping = shipping;
            cart.ShippingMethod = shippingMethod;
            cart.OrderDate = DateTime.Now;            

            // Refresh each product's price in the cart (in case the product price has changed)
            foreach (var orderProduct in cart.OrderProducts)
            {
                orderProduct.PriceOneCopy = orderProduct.Product.Price;
            }
            // Recalculate the total to ensure accuracy
            cart.TotalPrice = cart.OrderProducts.Sum(op => op.PriceOneCopy * op.Amount);
            await _db.SaveChangesAsync();
            return true;
        }

        // Deletes an order and its related order products from the database
        public async Task<bool> DeleteOrderAsync(int orderid)
        {
            var order = await _db.Orders
                .Include(o => o.OrderProducts)                
                .FirstOrDefaultAsync(o => o.Id == orderid);

            if (order == null)
            {
                return false; // Order not found
            }

            _db.OrderProducts.RemoveRange(order.OrderProducts);
            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            return true;
        }

        // Calculates the total price including shipping
        public decimal GetTotalPriceWithShipping(Order order)
        {
            return order.TotalPrice + order.Shipping;
        }
    }
}
