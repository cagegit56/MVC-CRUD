using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

    public interface IGetAllService
    {
       Task<List<Chat>> GetAllData();
    }

