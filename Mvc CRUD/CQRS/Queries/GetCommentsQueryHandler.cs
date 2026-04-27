using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Queries;

internal sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, List<CommentsDto>>
{
    private readonly DataDbContext _context;

    public GetCommentsQueryHandler(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<CommentsDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
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
                   Reply = x.Reply.Select(r => new CommentsReplyDto
                   {
                       Id = r.Id,
                       UserName = r.UserName,
                       LastName = r.LastName,
                       UserImageUrl = r.UserImageUrl,
                       Message = r.Message,
                       SentOn = r.SentOn,
                       CommentId = r.CommentId,
                       Replies = r.Replies.Select(y => new ReplyOfReplyDto
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
            return res;

            //var query = await _context.Comment.Include(x => x.Reply).Where(x => x.PostId == postId)
            //.AsSplitQuery().OrderByDescending(x => x.SentOn).AsNoTracking().ToListAsync();
            //var res = _mapper.Map<List<CommentsDto>>(query);
            //return Json(res);

        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to return all comments due to : {ex.Message}");
        }
    }
}

