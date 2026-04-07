using MediatR;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

internal sealed class GetUserInfoService : IGetUserInfoService
{
    private readonly DataDbContext _context;

    public GetUserInfoService(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<UserProfile?> GetUserInfo(string userId)
    {
        try
        {
            var res = await _context.Profile.Where(x => x.UserId == userId).FirstOrDefaultAsync();
            return res;
        }
        catch (Exception ex) {
            throw new Exception($"Failed to retrieve user info due to: {ex.Message}");
        }
    }
}

