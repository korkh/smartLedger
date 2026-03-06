using Microsoft.EntityFrameworkCore;

namespace Application.Core
{
    public class PagedList<T> : List<T>
    {
        public PagedList(IEnumerable<T> items, int count, int pageNumber, int pageSize)
        {
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);
            PageSize = pageSize;
            TotalCount = count;
            AddRange(items); //passing all items we get passed as a parameter into a class tha we gonna be returning, otherwise it will return 0.
        }

        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        //Creating a static method inside this class allows us to create a PageList and return items
        public static async Task<PagedList<T>> CreateAsync(
            IQueryable<T> source,
            int pageNumber,
            int pageSize
        )
        {
            var count = await source.CountAsync(); //total number of items in the list
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync(); //Skip is used to sett pagination
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
    }
}
