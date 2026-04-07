using Mvc_CRUD.Models;

namespace Mvc_CRUD.Services;

    public interface IGetUserInfoService
    {
      Task<UserProfile?> GetUserInfo(string userId);
    }

