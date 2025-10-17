using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Claims;

namespace Mvc_CRUD.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IGetAllService _getService;
        private readonly IPaginationService _pagination;
        private IMemoryCache _cache;

        public HomeController(ILogger<HomeController> logger, DataDbContext context,
            IGetAllService getService, IMemoryCache cache, IPaginationService pagination)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _getService = getService ?? throw new ArgumentNullException(nameof(getService));
            _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
            _cache = cache;
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] PaginationFilter pgFilter, string? filter)
        {
            try
            {
                string cacheKey = "cacheAll";
                if (!_cache.TryGetValue(cacheKey, out List<Chat>? res))
                {
                    res = await _context.Chats.AsNoTracking().ToListAsync();
                    await AddUser();
                    _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
                }


                IEnumerable<Chat>? queryData = res;
                if (!string.IsNullOrEmpty(filter))
                {
                    filter = filter.ToLower();
                    queryData = queryData.Where(x => x.UserName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        x.ToUser.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                        x.Message.Contains(filter, StringComparison.OrdinalIgnoreCase));
                }

                var paginatedRes = await _pagination.Paginate(queryData.ToList(), pgFilter);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.ContentType = "application/json";
                    return Json(new
                    {
                        data = paginatedRes.Data,
                        totalRecords = paginatedRes.TotalRecords,
                        totalPages = paginatedRes.TotalPages,
                        CurrentPage = pgFilter.PageNumber,
                        pageSize = pgFilter.PageSize
                    });
                }

                return View(paginatedRes);

            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = ex.Message
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> AddData(int? Id)
        {
          if(Id.HasValue && Id.Value > 0)
            {
                var res = await _context.Chats.SingleOrDefaultAsync(x => x.Id == Id);
                return View(res);
            }

          return View(new Chat());
        }

        [HttpPost]
        public async Task<IActionResult> AddData(Chat model)
        {
            if (ModelState.IsValid)
            {
               if (model.Id == 0)
                {
                  await _context.Chats.AddAsync(model);
                }
               else
                { 
                  _context.Chats.Update(model);               
                }

                await _context.SaveChangesAsync();
                _cache.Remove("cacheAll");
                return RedirectToAction("Index");
            }
            return View(model);
        }

        public async Task<IActionResult> DeleteData(int Id)
        {
            var rec = await _context.Chats.FirstOrDefaultAsync(x => x.Id == Id);
            _context.Chats.Remove(rec);
            await _context.SaveChangesAsync();
            _cache.Remove("cacheAll");
            TempData["Message"] = "Deleted record Successfully!";
            return Json(new { success = true, message = "Deleted successfully", id = Id });
        }

        public IActionResult UserProfile()
        {
            var user = User;

            var username = user.FindFirst(ClaimTypes.Name)?.Value
                           ?? user.FindFirst("preferred_username")?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                 ?? User.FindFirst("sub")?.Value;
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value
                     ?? User.FindFirst("given_name")?.Value;

            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value
                           ?? User.FindFirst("family_name")?.Value;

            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.UserId = userId;
            ViewBag.Username = username;
            ViewBag.Email = email;
            ViewBag.Roles = string.Join(", ", roles);

            return View();
        }

        public async Task AddUser()
        {
            var model = new Chat_Users();
            var user = User;
            model.UserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            bool existingUser = await _context.ChatUsers.AsNoTracking().AnyAsync(x => x.UserId == model.UserId);
            if (existingUser)
                return;

            model.UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value;
            model.FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value;
            model.LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value;
            model.Email = user.FindFirst(ClaimTypes.Email)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

            await _context.ChatUsers.AddAsync(model);
            await _context.SaveChangesAsync();
            Console.WriteLine("Data saved to users tablex");

        }

        [HttpGet]
        public async Task<IActionResult> SendingText(string? toFriend)
        {
            var user = User;
            var UserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            if (UserId.Count() == 0)
                return Logout();
            var Username = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value;
            var frnd = await _context.Friends.Where(x => x.UserId == UserId)
                                             .Select(x => x.FriendId)
                                             .ToListAsync();
            var res = new List<Chat>();
            foreach (var k in frnd)
            {
                var frndInfo = _context.ChatUsers.Where(x => x.UserId == k)
                                                  .Select(x => x.UserName);
                string friendName = frndInfo.FirstOrDefault();
                var msg = await _context.Chats.Where(x => x.UserName == Username && x.ToUser == friendName || x.UserName == friendName && x.ToUser == Username)
                                          .OrderByDescending(x => x.SentOn) 
                                          .ToListAsync();
                if (msg.Count == 0)
                {
                    var emptyMsg = new Chat()
                    {
                        UserName = Username,
                        ToUser = friendName,
                        Message = $"You are now Friends with {char.ToUpper(friendName[0]) + friendName.Substring(1)} Send a Message to Start a Chat",
                        SentOn = DateTime.UtcNow.AddMonths(-2),
                    };
                    res.Add(emptyMsg);
                }
                res.AddRange(msg);
            }

            var count = res.Count();
            
            if (!string.IsNullOrEmpty(toFriend))
            {
                res = res.Where(x => (x.UserName.Equals(Username, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(toFriend, StringComparison.OrdinalIgnoreCase))
                || (x.UserName.Equals(toFriend, StringComparison.OrdinalIgnoreCase) && x.ToUser.Equals(Username, StringComparison.OrdinalIgnoreCase)))
                    .OrderBy(x => x.SentOn)
                    .ToList();
                return Json(res);
            }
                 
            return View(res);
        }

        public IActionResult Logout()
        {
            return SignOut(
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("Index", "Home")
                },
                OpenIdConnectDefaults.AuthenticationScheme,
                CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
