using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Pagination;
using System.Linq.Expressions;
using System.Reflection;

namespace Mvc_CRUD.Services;

    public class PaginationService : IPaginationService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly IMapper _mapper;

        public PaginationService(IHttpContextAccessor httpContext, IMapper mapper)
        {
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<PaginateResponse<List<T>>> Paginate<T>(IQueryable<T> source, PaginationFilter filter,
           CancellationToken cancellationToken = default)
        {
            source = ApplySorting(source, filter.SortBy, filter.SortDirection);

            var totalRecords = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return CreatePaginatedResponse(items, totalRecords, filter);
        }

        public Task<PaginateResponse<List<T>>> Paginate<T>(IEnumerable<T> source, PaginationFilter filter,
           CancellationToken cancellationToken = default)
        {
            source = ApplySorting(source, filter.SortBy, filter.SortDirection);

            var totalRecords = source.Count();
            var items = source
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var response = CreatePaginatedResponse(items, totalRecords, filter);
            return Task.FromResult(response);
        }

        public async Task<PaginateResponse<List<TResponse>>> PaginateAndMap<TRequest, TResponse>(IQueryable<TRequest> source,
           PaginationFilter filter, CancellationToken cancellationToken = default)
        {
            source = ApplySorting(source, filter.SortBy, filter.SortDirection);
            var totalRecords = await source.CountAsync(cancellationToken);
            var items = await source
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var mappedData = _mapper.Map<List<TResponse>>(items);

            var response = CreatePaginatedResponse(mappedData, totalRecords, filter);

            return response;
        }

        public PaginateResponse<List<TResponse>> PaginateAndMap<TRequest, TResponse>(IEnumerable<TRequest> source,
           PaginationFilter filter, CancellationToken cancellationToken = default)
        {
            source = ApplySorting(source, filter.SortBy, filter.SortDirection);
            var totalRecords = source.Count();
            var items = source
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var mappedData = _mapper.Map<List<TResponse>>(items);

            var response = CreatePaginatedResponse(mappedData, totalRecords, filter);

            return response;
        }


        private static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string? sortBy, string? sortDirection)
        {
            var columnToSort = string.IsNullOrWhiteSpace(sortBy) ? "Id" : sortBy;

            var sortInfo = typeof(T).GetProperty(columnToSort,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sortInfo == null)
                return query;

            var param = Expression.Parameter(typeof(T), "x");
            var propertyAccess = Expression.Property(param, sortInfo);

            Expression converted = Expression.Convert(propertyAccess, typeof(object));
            var sortLambda = Expression.Lambda<Func<T, object>>(converted, param);

            return sortDirection?.ToLower() == "asc"
                ? query.OrderBy(sortLambda)
                : query.OrderByDescending(sortLambda);
        }

        private static IEnumerable<T> ApplySorting<T>(IEnumerable<T> query, string? sortBy, string? sortDirection)
        {
            var columnToSort = string.IsNullOrWhiteSpace(sortBy) ? "Id" : sortBy;

            var sortProperty = typeof(T).GetProperty(columnToSort,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sortProperty == null)
                return query;

            bool descending = !string.IsNullOrWhiteSpace(sortDirection) && sortDirection.ToLower() == "desc";

            return descending
                ? query.OrderBy(x => sortProperty.GetValue(x, null))
                : query.OrderByDescending(x => sortProperty.GetValue(x, null));
        }


        private PaginateResponse<List<T>> CreatePaginatedResponse<T>(List<T> items, int totalRecords,
                 PaginationFilter filter)
        {
            var httpContext = _httpContext.HttpContext
            ?? throw new InvalidOperationException("No active HttpContext found.");

            var response = new PaginateResponse<List<T>>(items, totalRecords, filter.PageNumber, filter.PageSize);

            var request = httpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}{request.Path}";

            if (httpContext.Response.Headers.ContainsKey("First-Page"))
                httpContext.Response.Headers.Remove("First-Page");
            if (httpContext.Response.Headers.ContainsKey("Last-Page"))
                httpContext.Response.Headers.Remove("Last-Page");
            if (httpContext.Response.Headers.ContainsKey("X-Total-Count"))
                httpContext.Response.Headers.Remove("X-Total-Count");

            var firstPageUrl = $"{baseUrl}?pageNumber=1&pageSize={response.PageSize}";
            var lastPageUrl = $"{baseUrl}?pageNumber={response.TotalPages}&pageSize={response.PageSize}";

            httpContext.Response.Headers.Add("First-Page", firstPageUrl);
            httpContext.Response.Headers.Add("Last-Page", lastPageUrl);
            httpContext.Response.Headers.Add("X-Total-Count", totalRecords.ToString());

            response.FirstPage = firstPageUrl;
            response.LastPage = lastPageUrl;

            return response;
        }
    }

