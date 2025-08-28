using Microsoft.EntityFrameworkCore;
using Auctionsite.Models;

namespace Auctionsite.Helpers
{
    public static class PaginationHelper
    {
        // Helper method to convert an IQueryable to a paginated result
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            // Maximum page size to prevent excessive data retrieval
            const int maxPageSize = 100;

            // Ensure page number is at least 1
            pageNumber = pageNumber < 1 ? 1 : pageNumber;

            // Ensure page size is within a reasonable limit. 
            // If page size is less than 1, set it to 10.
            // If page size is greater than maxPageSize, set it to maxPageSize.Otherwise, use the provided page size.
            pageSize = pageSize < 1 ? 10 : (pageSize > maxPageSize ? maxPageSize : pageSize); 

            var totalItems = await query.CountAsync();

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

    }

}
