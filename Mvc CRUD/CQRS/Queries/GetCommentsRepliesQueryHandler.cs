using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCommentsRepliesQueryHandler : IRequestHandler<GetCommentsRepliesQuery, PaginateResponse<List<CommentsReplyDto>>>
{
    private readonly DataDbContext _context;
    private readonly IPaginationService _pagination;
    private readonly ILogger<GetCommentsRepliesQueryHandler> _logger;

    public GetCommentsRepliesQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetCommentsRepliesQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PaginateResponse<List<CommentsReplyDto>>> Handle(GetCommentsRepliesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.commentId <= 0)
                return new PaginateResponse<List<CommentsReplyDto>>() { Error = "Comment Id cannot be null or 0." };
            var res = await _context.ReplyComments.Where(x => x.CommentId == request.commentId)
                .Select( g => new CommentsReplyDto() { 
                    Id = g.Id,
                    UserName = g.UserName,
                    LastName = g.LastName,
                    UserImageUrl = g.UserImageUrl,
                    Message = g.Message,
                    ImageContentUrl = g.ImageContentUrl,
                    CommentId = g.CommentId,
                    TotalReplies = g.TotalReplies,
                    SentOn = g.SentOn,
                    Replies = g.Replies.OrderByDescending(s => s.SentOn).Take(5).Select(r => new ReplyOfReplyDto() {
                        Id = r.Id,
                        UserName = r.UserName,
                        LastName = r.LastName,
                        UserImageUrl = r.UserImageUrl,
                        Message = r.Message,
                        ImageContentUrl = r.ImageContentUrl,
                        ReplyId = r.ReplyId,
                        SentOn = r.SentOn
                    }).ToList()
                }).AsNoTracking().ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, request.pgFilter);
            return paginatedRes;
        }
        catch (Exception ex) {
            _logger.LogError($"Failed to get comment replies due to {ex.Message}");
            return new PaginateResponse<List<CommentsReplyDto>>() { Error = "Failed to get comment replies, see inner exception for more info." };
        }        
    }
}
