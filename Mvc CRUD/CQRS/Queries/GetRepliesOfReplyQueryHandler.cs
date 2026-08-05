using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using System.Reflection.Metadata;

namespace Mvc_CRUD.CQRS.Queries;

    internal sealed class GetRepliesOfReplyQueryHandler : IRequestHandler<GetRepliesOfReplyQuery, PaginateResponse<List<ReplyOfReplyDto>>>
    {
        private readonly DataDbContext _context;
        private readonly IPaginationService _pagination;
        private readonly ILogger<GetRepliesOfReplyQueryHandler> _logger;

        public GetRepliesOfReplyQueryHandler(DataDbContext context, IPaginationService pagination, ILogger<GetRepliesOfReplyQueryHandler> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<PaginateResponse<List<ReplyOfReplyDto>>> Handle(GetRepliesOfReplyQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.replyId <= 0)
                    return new PaginateResponse<List<ReplyOfReplyDto>> { Error = "Reply Id cannot be null or 0." };
                var res = await _context.Replies.Where(x => x.ReplyId == request.replyId).AsNoTracking().ToListAsync();
                var paginatedRes = _pagination.PaginateAndMap<ReplyOfReply, ReplyOfReplyDto>(res, request.pgFilter, cancellationToken);
                return paginatedRes;
            }
            catch (Exception ex) 
            { 
                _logger.LogError($"Falied to get replies due to {ex.Message} ");
                return new PaginateResponse<List<ReplyOfReplyDto>> { Error = "Falied to get replies see inner exception for more info." };
            }            
        }
    }
