using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Auctionsite.Data;
using Auctionsite.Models.Database;
using Auctionsite.Models.VM;
using Auctionsite.Services.Interfaces;

namespace Auctionsite.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;
        public CategoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        // Get only the top-level categories
        public async Task<List<CategoryForAdvertisement>> GetParentCategories()
        {
            // Fetch root categories ordered by their display order
            return await _db.CategoryForAdvertisements
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        // Get all the top-level categories as SelectListItems
        public async Task<List<SelectListItem>> GetParentCategorySelectListItemsAsync()
        {
            // Convert root categories into SelectListItems for dropdowns
            var categories = await _db.CategoryForAdvertisements
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return categories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }

        // Get all the immediate subcategories of a parent category as entities (e g. "Electronics" -> "Mobile Phones", "Laptops")
        public async Task<List<CategoryForAdvertisement>> GetSubCategoriesAsync(int? parentId)
        {
            // Fetch child categories for a given parent, ordered by display order
            return await _db.CategoryForAdvertisements
                .Where(c => c.ParentCategoryId == parentId)
                .Include(c => c.Subcategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }

        // Get all the immediate subcategories of a parent category as SelectListItems
        public async Task<List<SelectListItem>> GetSubCategorySelectListItemsAsync(int parentId)
        {
            // Convert child categories into SelectListItems for dropdowns
            var subCategories = await _db.CategoryForAdvertisements
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return subCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            }).ToList();
        }

        // Get all categories (both parent and subcategories) as flat SelectListItems
        public async Task<List<SelectListItem>> GetAllCategoriesFlatAsync()
        {
            var all = await _db.CategoryForAdvertisements
                .Include(c => c.Subcategories)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var roots = all.Where(c => c.ParentCategoryId == null).OrderBy(c => c.DisplayOrder);

            var result = new List<SelectListItem>();
            void AddChildren(CategoryForAdvertisement cat, int level)
            {
                result.Add(new SelectListItem
                {
                    Value = cat.Id.ToString(),
                    Text = new string('–', level * 2) + " " + cat.Name
                });
                foreach (var child in all.Where(c => c.ParentCategoryId == cat.Id).OrderBy(c => c.DisplayOrder))
                {
                    AddChildren(child, level + 1);
                }
            }

            foreach (var root in roots)
                AddChildren(root, 0);

            return result;
        }


        // Get all categories (both parent and subcategories) as SelectListItems
        // In your CategoryService.cs

        public async Task<List<CategoryGroupVM>> GetGroupedSubCategoriesAsync()
        {
            var parentCategories = await _db.CategoryForAdvertisements
                .Where(c => c.ParentCategoryId == null)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var result = new List<CategoryGroupVM>();

            foreach (var parent in parentCategories)
            {
                var subCategories = await _db.CategoryForAdvertisements
                    .Where(c => c.ParentCategoryId == parent.Id)
                    .OrderBy(c => c.DisplayOrder)
                    .ToListAsync();

                if (subCategories.Any())
                {
                    result.Add(new CategoryGroupVM
                    {
                        GroupId = parent.Id, // <-- ADD THIS LINE
                        GroupName = parent.Name,
                        SubCategories = subCategories.Select(c => new SelectListItem
                        {
                            Value = c.Id.ToString(),
                            Text = c.Name
                        }).ToList()
                    });
                }
            }

            return result;
        }

        // Generate hierarchical codes (e.g., "1", "1.2", "1.2.3") for all categories
        public async Task<Dictionary<int, string>> GetCategoryCodesAsync()
        {
            // Load all categories and index by parent
            var categories = await _db.CategoryForAdvertisements
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var lookup = categories.ToLookup(c => c.ParentCategoryId);
            var codes = new Dictionary<int, string>();

            // Recursive traversal to build codes
            void Traverse(CategoryForAdvertisement category, string code)
            {
                codes[category.Id] = code;
                var children = lookup[category.Id].OrderBy(c => c.DisplayOrder).ToList();
                for (int i = 0; i < children.Count; i++)
                {
                    // Append child index to parent's code
                    Traverse(children[i], $"{code}.{i + 1}");
                }
            }

            // Start with root categories
            var roots = lookup[null].OrderBy(c => c.DisplayOrder).ToList();
            for (int i = 0; i < roots.Count; i++)
            {
                // Root code is its 1-based position
                Traverse(roots[i], (i + 1).ToString());
            }

            return codes;
        }

        // Get category breadcrumbs from root down to the specified category
        public async Task<List<CategoryForAdvertisement>> GetCategoryBreadcrumbsAsync(int categoryId)
        {
            // Retrieve all categories
            var all = await _db.CategoryForAdvertisements.ToListAsync();
            var crumbs = new List<CategoryForAdvertisement>();

            // Traverse upwards from the specified category to the root
            var current = all.FirstOrDefault(c => c.Id == categoryId);
            while (current != null)
            {
                crumbs.Add(current);
                if (current.ParentCategoryId == null)
                    break;
                current = all.FirstOrDefault(c => c.Id == current.ParentCategoryId);
            }

            // Reverse to get breadcrumbs from root to the selected category
            crumbs.Reverse();
            return crumbs;
        }

        // Get all descendant category IDs for a given category
        public async Task<List<int>> GetDescendantCategoryIdsAsync(int categoryId)
        {
            // Retrieve all categories
            var all = await _db.CategoryForAdvertisements
                               .Select(c => new { c.Id, c.ParentCategoryId })
                               .ToListAsync();

            // Create a lookup for parent-child relationships 
            var lookup = all.ToLookup(x => x.ParentCategoryId);

            // Recursive function to gather all descendant IDs
            var result = new List<int>();
            void Recurse(int id)
            {
                result.Add(id);
                foreach (var childId in lookup[id].Select(x => x.Id))
                    Recurse(childId);
            }
            // Start recursion from the given category ID
            Recurse(categoryId);
            return result;
        }
    }
}
