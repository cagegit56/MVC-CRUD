using FluentResults;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using System.Diagnostics;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class UpdateProfilePictureCommandHandler : IRequestHandler<UpdateProfilePictureCommand, Result>
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

    public async Task<Result> Handle(UpdateProfilePictureCommand request, CancellationToken cancellationToken)
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

                    string galleryFolder = Path.Combine("wwwroot/images/Gallery");
                    Directory.CreateDirectory(galleryFolder);
                    string galleryFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.image.FileName);
                    string galleryFilePath = Path.Combine(galleryFolder, galleryFileName);

                    using (var stream = new FileStream(galleryFilePath, FileMode.Create))
                    {
                        await request.image.CopyToAsync(stream);
                    }

                    var galleryInfo = new GalleryImages() 
                    { 
                        UserName = _currentUser.UserName!,
                        LastName = _currentUser.LastName!,
                        UserId = _currentUser.UserId!,
                        ImageUrl = "/images/Gallery/" + galleryFileName
                    };
                    await _context.Gallery.AddAsync(galleryInfo);

                    res.UserProfilePicUrl = "/images/ProfilePictures/" + fileName;
                    _context.Update(res);

                    var posts = await _context.Post.Where(x => x.UserId == _currentUser.UserId)
                        .ExecuteUpdateAsync(u => u.SetProperty(p => p.UserImageUrl, "/images/ProfilePictures/" + fileName));

                    var friendRequests = await _context.FriendRequests
                        .Where(x => x.UserId == _currentUser.UserId && x.Status == "Pending")
                        .ExecuteUpdateAsync(s => s.SetProperty(p => p.ProfilePicUrl, "/images/ProfilePictures/" + fileName));

                    await _context.SaveChangesAsync(cancellationToken);
                    await trans.CommitAsync(cancellationToken);
                }
                _cache.Remove($"UserInfo-{_currentUser.UserId}");
                return Result.Ok();
            }
            else
            {
                await trans.RollbackAsync(cancellationToken);
                _logger.LogError("Image content is null, cannot save an empty image.");
                return Result.Fail("Image content is null, cannot save an empty image.");
            }
        }
        catch (Exception ex) {
            await trans.RollbackAsync(cancellationToken);
            _logger.LogError($"Failed to update profile picture due to : {ex.Message}");
            return Result.Fail("Technical error, please see the inner exeception");
        }        
    }
}

