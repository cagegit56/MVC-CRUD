using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

public class GetAllService : IGetAllService
{
    private readonly DataDbContext _context;

    public GetAllService(DataDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }
    public async Task<List<Chat>> GetAllData()
    {
        var res = await _context.Chats.ToListAsync();
        return res;
    }
}

