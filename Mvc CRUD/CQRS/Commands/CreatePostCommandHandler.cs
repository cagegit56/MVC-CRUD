using MediatR;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using static System.Formats.Asn1.AsnWriter;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<CreatePostCommandHandler> _logger;

    public CreatePostCommandHandler(DataDbContext context, IMediator mediator, ILogger<CreatePostCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.model.Content) && (request.postImage == null || request.postImage.Length == 0))
            {
                _logger.LogError("Text content and Image content cannot both be null/empty");
                return false;
            }
            var model = new Posts();
            var CurrentUser = await _mediator.Send(new GetUserProfileQuery());
            if (CurrentUser.UserId != null && CurrentUser.UserName != null && CurrentUser.LastName != null)
            {
                model.UserId = CurrentUser.UserId;
                model.UserName = CurrentUser.UserName;
                model.LastName = CurrentUser.LastName;
                model.UserImageUrl = CurrentUser.UserProfilePicUrl;
            }
            else
            {
                _logger.LogError("Current user info cannot be null");
                return false;
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

                model.ImageContentUrl = "/images/PostPictures/" + fileName;
            }

            _context.Post.Add(model);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (Exception Ex)
        {
           _logger.LogError($"Failed to create a new post due to : {Ex.Message}");
            return false;
        }
    }
}

