using MediatR;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using static System.Formats.Asn1.AsnWriter;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, bool>
{
    private readonly DataDbContext _context;
    private readonly IMediator _mediator;

    public CreatePostCommandHandler(DataDbContext context, IMediator mediator)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mediator = mediator;
    }

    public async Task<bool> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(request.model.Content) && (request.postImage == null || request.postImage.Length == 0))
            {
                throw new Exception("Text content and Image content cannot both be null/empty");
            }
            var model = new Posts();
            var CurrentUser = await _mediator.Send(new GetUserProfileQuery());
            model.UserId = CurrentUser.UserId;
            model.UserName = CurrentUser.UserName!;
            model.LastName = CurrentUser.LastName!;
            model.UserImageUrl = CurrentUser.UserProfilePicUrl;
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
           throw new Exception($"Failed to create a new post due to : {Ex.Message}");
        }
    }
}

