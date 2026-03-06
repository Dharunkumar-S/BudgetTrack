namespace Budget_Track.Models.DTOs.Pagination
{
    public class PagedResult<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool HasNextPage => PageNumber < TotalPages;
        public bool HasPreviousPage => PageNumber > 1;
        public int FirstPage => 1;
        public int LastPage => TotalPages;
        public int? NextPage => HasNextPage ? PageNumber + 1 : null;
        public int? PreviousPage => HasPreviousPage ? PageNumber - 1 : null;
        public int FirstItemIndex => TotalRecords == 0 ? 0 : ((PageNumber - 1) * PageSize) + 1;
        public int LastItemIndex => Math.Min(PageNumber * PageSize, TotalRecords);
        public int CurrentPageItemCount => Data?.Count ?? 0;
        public bool IsFirstPage => PageNumber == 1;
        public bool IsLastPage => PageNumber == TotalPages;

        public static PagedResult<T> Create(
            List<T> data,
            int pageNumber,
            int pageSize,
            int totalRecords
        )
        {
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return new PagedResult<T>
            {
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = totalPages,
            };
        }
    }
}
