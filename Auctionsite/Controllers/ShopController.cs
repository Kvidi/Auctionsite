using Microsoft.AspNetCore.Mvc;
using Vega.Models.Database;
using Vega.Services.Interfaces;

namespace Vega.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        public ShopController(IProductService productService)
        {
            _productService = productService;
        }
        // Show all products
        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // Show product details by id
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _productService.ViewProductAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // Show add product form
        public IActionResult Create()
        {
            return View();
        }

        // Handle add product form submission
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            try
            {
                var result = await _productService.AddProductAsync(product);
                if (result)
                {
                    TempData["Success"] = "Product added successfully!";
                    return RedirectToAction("Index");
                }
                TempData["Error"] = "Failed to add product.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while adding the product.";
            }

            return View(product);
        }

        // Show edit product form
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["Error"] = "Product not found.";
                return RedirectToAction("Index");
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (!ModelState.IsValid)
                return View(product);

            try
            {
                // Get the product from the database
                var existingProduct = await _productService.GetProductByIdAsync(product.Id);
                if (existingProduct == null)
                {
                    TempData["Error"] = "Product not found.";
                    return RedirectToAction("Index");
                }

                //  fields that are allowed to be changed
                existingProduct.Title = product.Title;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.HeadImageURL = product.HeadImageURL;
                existingProduct.VideoURL = product.VideoURL;
                existingProduct.ImageList = product.ImageList;
                existingProduct.MomsSats = product.MomsSats;

                var result = await _productService.UpdateProductAsync(existingProduct);
                if (result)
                {
                    TempData["Success"] = "Product updated successfully!";
                    return RedirectToAction("Index");
                }

                TempData["Error"] = "Failed to update product.";
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating the product.";
            }

            return View(product);
        }
        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _productService.DeleteProductAsync(id);
                if (result)
                {
                    TempData["Success"] = "Product deleted successfully!";
                }
                else
                {
                    TempData["Error"] = "Product not found or could not be deleted.";
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while deleting the product.";
            }

            return RedirectToAction("Index");
        }

        //  Filter by category, price, search, etc.
        public async Task<IActionResult> FilterByPrice(decimal min, decimal max)
        {
            var products = await _productService.GetProductsByPriceRangeAsync(min, max);
            return View("Index", products);
        }
        public async Task<IActionResult> FilterByDate(DateTime start, DateTime end)
        {
            var products = await _productService.GetProductsByDateRangeAsync(start, end);
            return View("Index", products);
        }
        public async Task<IActionResult> FilterByCategory(int categoryId)
        {
            var products = await _productService.GetProductsByCategoryIdAsync(categoryId);
            return View("Index", products);
        }
        public async Task<IActionResult> Search(string searchTerm)
        {
            var products = await _productService.GetProductsBySearchTermAsync(searchTerm);
            return View("Index", products);
        }

        public async Task<IActionResult> Sort(string sortBy, bool isLowToHigh)
        {
            var products = await _productService.GetSortedProductsAsync(sortBy, isLowToHigh);
            return View("Index", products);
        }
    }
}
