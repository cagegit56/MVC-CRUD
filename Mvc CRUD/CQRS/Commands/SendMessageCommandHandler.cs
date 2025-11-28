using MediatR;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.CQRS.Commands;

    internal sealed class SendMessageCommandHandler : IRequestHandler<SendMessageCommand, string>
    {
        private readonly DataDbContext _context;

        public SendMessageCommandHandler(DataDbContext context)
        {
           _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public async Task<string> Handle(SendMessageCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var res = await _context.Chats.AddAsync(command.model);
                await _context.SaveChangesAsync();
                return "Successfully Saved";
            }
            catch (Exception ex) 
            {
                return $"Failed to send message due to : {ex.Message}";
            }
           
        }
    }

