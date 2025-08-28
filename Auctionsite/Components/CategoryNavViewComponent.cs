using Microsoft.AspNetCore.Mvc;
using Auctionsite.Services.Interfaces; // Make sure you have the correct using statement for your service

namespace Auctionsite.ViewComponents
{
    public class CategoryNavViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public CategoryNavViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // 1. Call the service method that returns your List<CategoryGroupVM>
            var categories = await _categoryService.GetGroupedSubCategoriesAsync();

            // 2. Pass this list to the component's view
            return View(categories);
        }
    }
}