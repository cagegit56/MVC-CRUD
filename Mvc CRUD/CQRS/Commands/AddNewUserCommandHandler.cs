using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;
using Mvc_CRUD.Services;

namespace Mvc_CRUD.CQRS.Commands;

internal sealed class AddNewUserCommandHandler : IRequestHandler<AddNewUserCommand, (bool success, string error)>
{
    private readonly DataDbContext _context;
    private readonly IUserInfoContextService _currentUserInfo;

    public AddNewUserCommandHandler(DataDbContext context, IUserInfoContextService currentUserInfo)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _currentUserInfo = currentUserInfo ?? throw new ArgumentNullException(nameof(_currentUserInfo));
    }
    public async Task<(bool success, string error)> Handle(AddNewUserCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            bool existingUser = await _context.ChatUsers.AsNoTracking().AnyAsync(x => x.UserId == _currentUserInfo.UserId);
            if (!existingUser)
            {
                var model = new Chat_Users
                {
                    UserId = _currentUserInfo.UserId!,
                    UserName = _currentUserInfo.UserName,
                    FirstName = _currentUserInfo.FirstName!,
                    LastName = _currentUserInfo.LastName,
                    Email = _currentUserInfo.Email!,
                    //Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
                };
                await _context.ChatUsers.AddAsync(model);
            }

            var existingProfile = await _context.Profile.AsNoTracking().AnyAsync(x => x.UserId == _currentUserInfo.UserId);
            if (!existingProfile)
            {
                var profileModel = new UserProfile()
                {
                    UserId = _currentUserInfo.UserId!,
                    UserName = _currentUserInfo.UserName!,
                    LastName = _currentUserInfo.LastName!
                };
                await _context.Profile.AddAsync(profileModel);
            }

            if (!existingUser || !existingProfile)
            {
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            return (true, "SuccessFully Saved");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"Unable to save User's Info due to : {ex.Message}");
        }
    }
}

