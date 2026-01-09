namespace DapperLabs_Lab2.DTOs
{
    // [API] PagedResultDto - API-specific pagination response model
    public class PagedResultDto<T>
    {
        public IEnumerable<T> Items { get; set; } = new List<T>(); // [API] Current page items
        public int TotalCount { get; set; } // [API] Total items for pagination UI
        public int Page { get; set; } // [API] Current page number
        public int PageSize { get; set; } // [API] Items per page
        
        // [API] Calculated properties for pagination UI
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
        public bool HasPrevious => Page > 1;
        public bool HasNext => Page < TotalPages;
    }
}
