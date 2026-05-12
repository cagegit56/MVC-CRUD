using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class GalleryCommandHandler : IRequestHandler<GalleryCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly ILogger<GalleryCommandHandler> _logger;

    public GalleryCommandHandler(DataDbContext context, ILogger<GalleryCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
    }

    public async Task<bool> Handle(GalleryCommand request, CancellationToken cancellationToken)
    {
        //Add a db table for gallery
        try
        {
            if (request.image != null && request.image.Length > 0)
            {
                string folder = Path.Combine("wwwroot/images/Gallery");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.image.CopyToAsync(stream);
                }

                //model.ImageContentUrl = "/images/Gallery/" + fileName;
            }
            else
            {
                _logger.LogError("Image content cannot be null");
                return false;
            }

            //_context.Post.Add(model);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to add an image to gallery due to: {ex.Message}");
            return false;
        }
      
    }
}

