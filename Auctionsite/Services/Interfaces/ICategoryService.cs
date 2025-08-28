using Microsoft.AspNetCore.Mvc.Rendering;
using System.Threading.Tasks;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;

namespace Auctionsite.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryForAdvertisement>> GetParentCategories();
        Task<List<SelectListItem>> GetParentCategorySelectListItemsAsync();
        Task<List<CategoryForAdvertisement>> GetSubCategoriesAsync(int? parentId);
        Task<List<SelectListItem>> GetSubCategorySelectListItemsAsync(int parentId);
        Task<List<SelectListItem>> GetAllCategoriesFlatAsync();
        Task<List<CategoryGroupVM>> GetGroupedSubCategoriesAsync();
        Task<Dictionary<int, string>> GetCategoryCodesAsync();
        Task<List<CategoryForAdvertisement>> GetCategoryBreadcrumbsAsync(int categoryId);
        Task<List<int>> GetDescendantCategoryIdsAsync(int categoryId);
    }
}
