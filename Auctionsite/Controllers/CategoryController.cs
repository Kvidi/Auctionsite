using Microsoft.AspNetCore.Mvc;
using Auctionsite.Services.Interfaces; // Or wherever your ICategoryService is

namespace Auctionsite.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;

        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [Route("kategorier")] // This will be the URL: site.com/kategorier
        public async Task<IActionResult> Index()
        {
            //  method to get parents with their subcategories
            var groupedCategories = await _categoryService.GetGroupedSubCategoriesAsync();
            return View(groupedCategories);
        }
    }
}
