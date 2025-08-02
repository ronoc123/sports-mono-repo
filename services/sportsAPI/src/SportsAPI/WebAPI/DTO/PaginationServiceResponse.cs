using Microsoft.EntityFrameworkCore;

namespace sportsAPI.DTO
{
    public class PaginationServiceResponse<T>
    {
        public List<T> Items { get; set; }
        public int PageIndex { get; set; }
        public int TotalPages { get; set; }
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public PaginationServiceResponse(List<T> items, int pageIndex, int totalPages)
        {
            Items = items;
            PageIndex = pageIndex;
            TotalPages = totalPages;
        }

        // This static method helps to calculate pagination logic
        public static async Task<PaginationServiceResponse<T>> CreateResponseAsync(IQueryable<T> query, int pageIndex, int pageSize)
        {
            // Get the total count of records
            var totalCount = await query.CountAsync();

            // Get the records for the current page
            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return new PaginationServiceResponse<T>(items, pageIndex, totalPages);
        }
    }
}
