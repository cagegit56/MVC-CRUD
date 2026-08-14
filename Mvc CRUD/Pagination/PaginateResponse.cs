namespace Mvc_CRUD.Pagination;

    public class PaginateResponse<T>
    {
        public T Data { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public bool PreviousPage => PageNumber > 1;
        public bool NextPage => PageNumber < TotalPages;
        public string? FirstPage { get; set; }
        public string? LastPage { get; set; }
        public string? Error { get; set; }
        public string? UserName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicUrl { get; set; }

        public PaginateResponse() { }

        public PaginateResponse(T data, int totalRecords, int pageNumber, int pageSize)
        {
            PageNumber = pageNumber;
            PageSize = pageSize;
            TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            Data = data;
            TotalRecords = totalRecords;
        }
    }

