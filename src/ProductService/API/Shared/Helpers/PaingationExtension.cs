using Microsoft.EntityFrameworkCore;

namespace ProductService.API.Shared.Helpers
{
    public class PaingationExtension
    {
        public PagedResult<T> Paginate<T>(IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var items = query.Skip((pageNumber - 1) * pageSize)
                             .Take(pageSize)
                             .ToList();

            return new PagedResult<T>(items.AsQueryable(), pageNumber, pageSize, totalItems);

        }
    }

    public static class PaginationExtensions
    {
        public static async Task<PagedResult<T>> PaginateAsync<T>(this IQueryable<T> query, int pageNumber, int pageSize)
        {
            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new PagedResult<T>(new List<T>().AsQueryable(), 1, pageSize, 0);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                pageNumber = totalPages;

            var items = await query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();

            return new PagedResult<T>(items.AsQueryable(), pageNumber, pageSize, totalCount);
        }

        public static async Task<PagedResult<T>> PaginateAsync<T>(List<T> query, int pageNumber, int pageSize)
        {
            var totalCount = query.Count();

            if (totalCount == 0)
            {
                return new PagedResult<T>(new List<T>().AsQueryable(), 1, pageSize, 0);
            }

            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            if (pageNumber < 1)
                pageNumber = 1;
            else if (pageNumber > totalPages)
                pageNumber = totalPages;

            var items = query.Skip((pageNumber - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToList();

            return new PagedResult<T>(items.AsQueryable(), pageNumber, pageSize, totalCount);
        }
    }

    public class PagedResult<T>
    {
        public PagedResult(
            IQueryable<T> items,
            int pageNumber,
            int pageSize,
            int totalCount
            )
        {
            Items = items;
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        public IQueryable<T> Items { get; }
        public int PageNumber { get; }
        public int PageSize { get; }
        public int TotalCount { get; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }


}
