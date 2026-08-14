using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Pagination;
using System.Linq.Expressions;
using System.Reflection;

namespace Mvc_CRUD.Services;

    // **** works but does not return first and last page url, bcoz static class/methods cannot have imapper,
    // httpcontextaccessor or a constructor to initialize or instantiate 
    public static class staticPaginationTest
    {
        public static async Task<PaginateResponse<List<T>>> Paginate<T>(IQueryable<T> source, PaginationFilter filter,
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

        public static Task<PaginateResponse<List<T>>> Paginate<T>(IEnumerable<T> source, PaginationFilter filter,
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

        //public static async Task<PaginateResponse<List<TResponse>>> PaginateAndMap<TRequest, TResponse>(IQueryable<TRequest> source,
        //   PaginationFilter filter, CancellationToken cancellationToken)
        //{
        //    source = ApplySorting(source, filter.SortBy, filter.SortDirection);
        //    var totalRecords = await source.CountAsync(cancellationToken);
        //    var items = await source
        //        .Skip((filter.PageNumber - 1) * filter.PageSize)
        //        .Take(filter.PageSize)
        //        .Select(x => new List<TResponse>)
        //        .ToListAsync(cancellationToken);

        //    var mappedData = _mapper.Map<List<TResponse>>(items);

        //    var response = CreatePaginatedResponse(mappedData, totalRecords, filter);

        //    return response;
        //}

        //public static PaginateResponse<List<TResponse>> PaginateAndMap<TRequest, TResponse>(IEnumerable<TRequest> source,
        //   PaginationFilter filter, CancellationToken cancellationToken)
        //{
        //    source = ApplySorting(source, filter.SortBy, filter.SortDirection);
        //    var totalRecords = source.Count();
        //    var items = source
        //        .Skip((filter.PageNumber - 1) * filter.PageSize)
        //        .Take(filter.PageSize)
        //        .ToList();

        //    var mappedData = _mapper.Map<List<TResponse>>(items);
        //    var response = CreatePaginatedResponse(mappedData, totalRecords, filter);
        //    return response;
        //}


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


        private static PaginateResponse<List<T>> CreatePaginatedResponse<T>(List<T> items, int totalRecords,
                 PaginationFilter filter)
        {
            var response = new PaginateResponse<List<T>>(items, totalRecords, filter.PageNumber, filter.PageSize);
            return response;
        }

    }

