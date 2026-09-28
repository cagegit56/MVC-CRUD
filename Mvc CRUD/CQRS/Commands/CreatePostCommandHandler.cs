using FluentResults;
using MediatR;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;
using static System.Formats.Asn1.AsnWriter;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result>
{
    private readonly DataDbContext _context;
    private readonly ICurrentUserProfile _currentUser;
    private readonly ILogger<CreatePostCommandHandler> _logger;

    public CreatePostCommandHandler(DataDbContext context, ICurrentUserProfile currentUser, ILogger<CreatePostCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        _logger = logger;
    }

    public async Task<Result> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.model.Content) && (request.postImage == null || request.postImage.Length == 0))
            {
                _logger.LogError("Text content and Image content cannot both be null/empty");
                return Result.Fail("Text content and Image content cannot both be null/empty");
            }
            var model = new Posts();
            if (_currentUser.UserId != null && _currentUser.UserName != null && _currentUser.LastName != null)
            {
                model.UserId = _currentUser.UserId;
                model.UserName = _currentUser.UserName;
                model.LastName = _currentUser.LastName;
                model.UserImageUrl = _currentUser.ProfilePicUrl;
            }

            model.PostScope = request.model.PostScope;
            model.Content = request.model.Content;
            model.PostBgColour = request.model.PostBgColour;
            if (request.postImage != null && request.postImage.Length > 0)
            {
                string folder = Path.Combine("wwwroot/images/PostPictures");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.postImage.FileName);
                string filePath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await request.postImage.CopyToAsync(stream);
                }

                string galleryFolder = Path.Combine("wwwroot/images/Gallery");
                Directory.CreateDirectory(galleryFolder);
                string galleryFileName = Guid.NewGuid().ToString() + Path.GetExtension(request.postImage.FileName);
                string galleryFilePath = Path.Combine(galleryFolder, galleryFileName);

                using (var stream = new FileStream(galleryFilePath, FileMode.Create))
                {
                    await request.postImage.CopyToAsync(stream);
                }

                var galleryInfo = new GalleryImages()
                {
                    UserName = _currentUser.UserName!,
                    LastName = _currentUser.LastName!,
                    UserId = _currentUser.UserId!,
                    ImageUrl = "/images/Gallery/" + galleryFileName
                };
                await _context.Gallery.AddAsync(galleryInfo);

                model.ImageContentUrl = "/images/PostPictures/" + fileName;
            }

            _context.Post.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
            return Result.Ok();
        }
        catch (Exception Ex)
        {
           _logger.LogError($"Failed to create a new post due to : {Ex.Message}");
            return Result.Fail("Failed to create a new post due to a technical issue.");
        }
    }
}

