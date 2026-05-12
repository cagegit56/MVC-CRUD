using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateCoverPictureCommandHandler : IRequestHandler<UpdateCoverPictureCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UpdateCoverPictureCommandHandler> _logger;

    public UpdateCoverPictureCommandHandler(DataDbContext context, IUserInfoContextService currentUser, IMemoryCache cache, ILogger<UpdateCoverPictureCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateCoverPictureCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.image.Length > 0 && request.image != null)
            {
                var res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId).FirstOrDefaultAsync();
                if (res != null)
                {
                    string folder = Path.Combine("wwwroot/images/CoverPictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.image.CopyToAsync(stream);
                    }

                    res.UserCoverPicUrl = "/images/CoverPictures/" + fileName;
                    _context.Update(res);
                    await _context.SaveChangesAsync(cancellationToken);
                } 
                _cache.Remove($"UserInfo-{_currentUser.UserId}");
                return true;
            }
            else
            {
                _logger.LogError("No image content found/ image content cannot be null.");
                return false;
            }
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to save image content due to : {ex.Message}");
            return false;
        }        
    }
}

