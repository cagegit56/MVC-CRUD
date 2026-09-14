using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetGalleryImagesQueryHandler : IRequestHandler<GetGalleryImagesQuery, PaginateResponse<List<GalleryImages>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private readonly ILogger<GetGalleryImagesQueryHandler> _logger;

    public GetGalleryImagesQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetGalleryImagesQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginateResponse<List<GalleryImages>>> Handle(GetGalleryImagesQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.userId))
            return new PaginateResponse<List<GalleryImages>>() { Error = "UserId cannot be null.." };
        try
        {
            request.pgFilter.PageSize = 3;
            return await _pagination.Paginate( await _context.Gallery
                .Where(x => x.UserId == request.userId).AsNoTracking().ToListAsync(), 
                request.pgFilter, cancellationToken);
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to return gallery images dues to : {ex.Message}");
            return new PaginateResponse<List<GalleryImages>>() { Error = "Failed to return gallery images, Technical issue."};
        }
    }
}

