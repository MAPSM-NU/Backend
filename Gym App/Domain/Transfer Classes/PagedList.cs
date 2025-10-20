using Microsoft.EntityFrameworkCore;

namespace Gym_App.Domain.Transfer_Classes
{
    public class PagedList<T>
    {
        public PagedList(List<T> items,int page, int pageSize, int totalCount)
        {
            Items = items;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
        public List<T> Items { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public bool HasNextPage  => Page * PageSize < TotalCount;
        public bool HasPreviousPage => PageSize > 1;
        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> query,
            int page,
            int pageSize)
        {
            int totalCount = await query.CountAsync();
            var items = await query.Skip(pageSize*(page-1)).Take(pageSize).ToListAsync();

            return new(items, page, pageSize, totalCount);
        }

    }
}
