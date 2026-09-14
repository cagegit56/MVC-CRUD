using FluentResults;
using MediatR;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using static System.Net.Mime.MediaTypeNames;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class GalleryCommandHandler : IRequestHandler<GalleryCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly ILogger<GalleryCommandHandler> _logger;

    public GalleryCommandHandler(DataDbContext context, IUserInfoContextService currentUser, ILogger<GalleryCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }

    public async Task<Result> Handle(GalleryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.image != null && request.image.Length > 0)
            {
                var model = new GalleryImages() 
                {
                    UserName = _currentUser.UserName!,
                    LastName = _currentUser.LastName!,
                    UserId = _currentUser.UserId!,
                };
                string folder = Path.Combine("wwwroot/images/Gallery");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.image.CopyToAsync(stream);
                }

                model.ImageUrl = "/images/Gallery/" + fileName;

                await _context.Gallery.AddAsync(model);
                await _context.SaveChangesAsync();
                return Result.Ok();
            }
            else
            {
                _logger.LogError("Image content cannot be null");
                return Result.Fail("Image content cannot be null");
            }           
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to add an image to gallery due to: {ex.Message}");
            return Result.Fail("Failed to add an image to gallery due to a technical issue");
        }
      
    }
}

