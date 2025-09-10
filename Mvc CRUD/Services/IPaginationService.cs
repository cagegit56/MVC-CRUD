using Mvc_CRUD.Pagination;

namespace Mvc_CRUD.Services;

    public interface IPaginationService
    {
        Task<PaginateResponse<List<T>>> Paginate<T>(IQueryable<T> source, PaginationFilter filter,
            CancellationToken cancellationToken = default);
        Task<PaginateResponse<List<T>>> Paginate<T>(IEnumerable<T> source, PaginationFilter filter,
          CancellationToken cancellationToken = default);

        Task<PaginateResponse<List<TResponse>>> PaginateAndMap<TRequest, TResponse>(IQueryable<TRequest> source,
          PaginationFilter filter, CancellationToken cancellationToken = default);

        PaginateResponse<List<TResponse>> PaginateAndMap<TRequest, TResponse>(IEnumerable<TRequest> source,
          PaginationFilter filter,
          CancellationToken cancellationToken = default);
    }

