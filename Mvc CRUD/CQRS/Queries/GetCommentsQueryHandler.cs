using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, PaginateResponse<List<CommentsDto>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private readonly ILogger<GetCommentsQueryHandler> _logger;

    public GetCommentsQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetCommentsQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger;
    }

    public async Task<PaginateResponse<List<CommentsDto>>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var res = await _context.Comment.Where(x => x.PostId == request.postId)
               .Select(x => new CommentsDto
               {
                   Id = x.Id,
                   UserName = x.UserName,
                   LastName = x.LastName,
                   UserImageUrl = x.UserImageUrl,
                   Message = x.Message,
                   PostId = x.PostId,
                   SentOn = x.SentOn,
                   TotalComment = 
                   Reply = x.Reply.Take(5).Select(r => new CommentsReplyDto
                   {
                       Id = r.Id,
                       UserName = r.UserName,
                       LastName = r.LastName,
                       UserImageUrl = r.UserImageUrl,
                       Message = r.Message,
                       SentOn = r.SentOn,
                       CommentId = r.CommentId,
                       Replies = r.Replies.Take(5).Select(y => new ReplyOfReplyDto
                       {
                           Id = y.Id,
                           UserName = y.UserName,
                           LastName = y.LastName,
                           UserImageUrl = y.UserImageUrl,
                           Message = y.Message,
                           ReplyId = y.ReplyId,
                           SentOn = y.SentOn,
                       }).OrderByDescending(y => y.SentOn).ToList(),
                   }).OrderByDescending(r => r.SentOn).ToList()

               }).OrderByDescending(x => x.SentOn).AsNoTracking().ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to retrieve comments for post id {request.postId} due to {ex.Message}");
            return new PaginateResponse<List<CommentsDto>>() { Error = "Failed to retrieve comments." };
        }
    }
}

