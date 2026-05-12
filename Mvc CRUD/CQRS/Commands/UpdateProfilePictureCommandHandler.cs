using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Diagnostics;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUser;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UpdateProfilePictureCommandHandler> _logger;

    public UpdateProfilePictureCommandHandler(DataDbContext context, IUserInfoContextService currentUser, IMemoryCache cache, ILogger<UpdateProfilePictureCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger;
    }

    public async Task<bool> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
    {
        var trans = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            if (request.image.Length > 0 && request.image != null)
            {
                var res = await _context.Profile.Where(x => x.UserId == _currentUser.UserId).FirstOrDefaultAsync();
                if (res != null)
                {
                    string folder = Path.Combine("wwwroot/images/ProfilePictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.image.CopyToAsync(stream);
                    }

                    res.UserProfilePicUrl = "/images/ProfilePictures/" + fileName;
                    _context.Update(res);

                    var posts = await _context.Post.Where(x => x.UserId == _currentUser.UserId).ToListAsync();
                    foreach (var postInfo in posts)
                    {
                        postInfo.UserImageUrl = "/images/CoverPictures/" + fileName;
                        _context.Post.Update(postInfo);
                    }

                    await _context.SaveChangesAsync(cancellationToken);
                    await trans.CommitAsync(cancellationToken);
                }
                _cache.Remove($"UserInfo-{_currentUser.UserId}");
                return true;
            }
            else
            {
                await trans.RollbackAsync(cancellationToken);
                _logger.LogError("Image content is null, cannot save an empty image.");
                return false;
            }
        }
        catch (Exception ex) {
            await trans.RollbackAsync(cancellationToken);
            _logger.LogError($"Failed to update profile picture due to : {ex.Message}");
            return false;
        }        
    }
}

