namespace Mvc_CRUD.Pagination;

    public class PaginationFilter
    {
        private int _pageSize;
        public int PageNumber { get; set; } = 1;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > 5) ? 5 : value;
        }

        public PaginationFilter()
        {
            PageNumber = 1;
            PageSize = 5;
        }

        public PaginationFilter(int pageNumber, int pageSize, string sortBy, string sortDirection)
        {
            PageNumber = pageNumber < 1 ? 1 : pageNumber;
            PageSize = pageSize > 5 ? 5 : pageSize;
            SortBy = sortBy;
            SortDirection = sortDirection;
        }
    }

