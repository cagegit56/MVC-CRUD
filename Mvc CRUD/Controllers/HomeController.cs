using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.CQRS.Commands;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Linq;
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
        private readonly IMediator _mediator;
        private readonly IHttpContextAccessor _httpContext;
        private string _currentUserId;
        private string _currentUserName;

        public HomeController(ILogger<HomeController> logger, DataDbContext context,
            IGetAllService getService, IMemoryCache cache, IPaginationService pagination, IMediator mediator, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _getService = getService ?? throw new ArgumentNullException(nameof(getService));
            _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _cache = cache;
            _mediator = mediator;
            _currentUserId = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? _httpContext.HttpContext.User.FindFirst("sub").Value;
            _currentUserName = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? _httpContext.HttpContext.User.FindFirst("preferred_username").Value;
        }

        
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] PaginationFilter pgFilter, string filter)
        {
            var res = await _mediator.Send(new GetAllQuery(pgFilter, filter));
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.ContentType = "application/json";
                return Json(new
                {
                    data = res.Data,
                    totalRecords = res.TotalRecords,
                    totalPages = res.TotalPages,
                    CurrentPage = pgFilter.PageNumber,
                    pageSize = pgFilter.PageSize
                });
            }
            await AddUser();
            return View(res);
        }

        [HttpGet]
        [Authorize]
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
        [Authorize]
        public async Task<IActionResult> AddData(Chat model)
        {
            Console.WriteLine(model);
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

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(model);
            }

                return View(model);
        }

        [Authorize]
        public async Task<IActionResult> DeleteData(int Id)
        {
            var rec = await _context.Chats.FirstOrDefaultAsync(x => x.Id == Id);
            _context.Chats.Remove(rec);
            await _context.SaveChangesAsync();
            _cache.Remove("cacheAll");
            TempData["Message"] = "Deleted record Successfully!";
            return Json(new { success = true, message = "Deleted successfully", id = Id });
        }

        [Authorize]
        public IActionResult UserProfile()
        {
            var user = User;

            var username = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value;
            var email = user.FindFirst(ClaimTypes.Email)?.Value;
            var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
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

        [Authorize]
        public async Task<IActionResult> AddUser()
        {
            try
            {
                var model = new Chat_Users();
                var user = User;
                model.UserId = _currentUserId;
                bool existingUser = await _context.ChatUsers.AsNoTracking().AnyAsync(x => x.UserId == model.UserId);
                if (existingUser)
                    return BadRequest("User Already Exists...");

                model.UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value;
                model.FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value;
                model.LastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value;
                model.Email = user.FindFirst(ClaimTypes.Email)?.Value;
                var roles = user.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

                await _context.ChatUsers.AddAsync(model);
                await _context.SaveChangesAsync();
                Console.WriteLine("Data saved to users table");
                return Ok("Saved Successfully");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Unable to save User's Info : {ex.Message}");
                return BadRequest($"Unable to save User's Info : {ex.Message}");
            }

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Chats(string toFriend)
        {
            var res = await _mediator.Send(new GetChatsQuery(_currentUserId, _currentUserName, toFriend));
            if (!string.IsNullOrEmpty(toFriend))
                return Json(res);
                return View(res);
        }

        [Authorize]
        public async Task<IActionResult> friendRequests([FromQuery] PaginationFilter pgFilter)
        {
            var friends = await _context.Friends.Where(x => x.UserId == _currentUserId || x.FriendId == _currentUserId).Select(x => x.UserId).ToListAsync();
            var blockedUser = await _context.BlockedUser.Where(x => x.UserId == _currentUserId).Select(x => x.BlockUserId).ToListAsync();
            var allUsers = await _context.ChatUsers.Where(x => x.UserId != _currentUserId && !friends.Contains(x.UserId) && !blockedUser.Contains(x.UserId)).ToListAsync();
            var paginatedRes = await _pagination.Paginate(allUsers, pgFilter);
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> addFriends(string userId, string frndName)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = User;
                var currentUserId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
                var Username = user?.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("preferred_username")?.Value;
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(frndName))
                    return Json(new { success = false, message = "Friend's Id or Friend's Name is empty." });
                bool exists = await _context.Friends.AnyAsync(x =>
                    (x.UserId == currentUserId && x.FriendId == userId) ||
                    (x.UserId == userId && x.FriendId == currentUserId)
                );
                if (exists)
                    return Json(new { success = false, message = "Friendship already exists." });
                var frnd = new Friends()
                {
                    UserId = userId,
                    FriendId = currentUserId,
                    Status = "online",
                    FriendName = Username,
                    UserName = frndName
                };
                var frnd2 = new Friends()
                {
                    UserId = currentUserId,
                    FriendId = userId,
                    Status = "online",
                    FriendName = frndName,
                    UserName = Username
                };
                await _context.Friends.AddRangeAsync(frnd, frnd2);
                await _context.SaveChangesAsync();

                var accepted = await _context.FriendRequests.Where(x => x.UserId == userId && x.ToUserId == currentUserId).FirstOrDefaultAsync();
                if (accepted != null)
                {
                    accepted.Status = "Accepted";
                    _context.FriendRequests.Update(accepted);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Json(new { success = true, message = "Friend Request Accepted" });

            }catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        [Authorize]
        public async Task<IActionResult> SendDbMessage(Chat model, string username, string friend, string message)
        {
            model.UserName = username;
            model.ToUser = friend;
            model.Message = message;
            var res = await _context.Chats.AddAsync(model);
            await _context.SaveChangesAsync();
            return Json("Success");
        }

        [Authorize]
        public async Task<IActionResult> request(string toUserId, string toUserName, string email)
        {
            try
            {
                var currentUser = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
                var model = new FriendRequest()
                {
                    UserId = currentUser,
                    UserName = User?.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("preferred_username").Value,
                    ToUserId = toUserId,
                    ToUserName = toUserName,
                    ToUserEmail = email
                };
                var exists = await _context.FriendRequests.Where(x => x.UserId == currentUser && x.ToUserId == toUserId ||
                              x.UserId == toUserId && x.ToUserId == currentUser).FirstOrDefaultAsync();
                if (exists != null)
                _context.FriendRequests.Remove(exists);
                await _context.FriendRequests.AddAsync(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Friend Request Successfully Sent" });
            }
            catch(Exception ex)
            {
                return Json($"Failed to send a friend request due to : {ex.Message}");
            }
          
        }

        [Authorize]
        public async Task<IActionResult> RecievedFriendRequest(PaginationFilter filter)
        {
            var currentUser = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            var res = await _context.FriendRequests.Where(x => x.ToUserId == currentUser && x.Status == "Pending" && x.isDeleted != true).ToListAsync();
            var paginatedRes = await _pagination.Paginate(res, filter);
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.ContentType = "application/json";
                return Json(new
                {
                    data = paginatedRes.Data,
                    totalRecords = paginatedRes.TotalRecords,
                    totalPages = paginatedRes.TotalPages,
                    CurrentPage = filter.PageNumber,
                    pageSize = filter.PageSize
                });
            }
            return View(paginatedRes);
        }

        [Authorize]
        public async Task<IActionResult> GetAllSentRequest(PaginationFilter filter)
        {
            var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            var res = await _mediator.Send(new GetAllSentRequestQuery(filter, currentUserId));
            return Json(res);
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> RejectRequest(string friendUserId)
        {
            var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            var res = await _mediator.Send(new RejectRequestCommand(currentUserId, friendUserId));
            if (res != "Successfully Cancelled")
                return Json(new { success = false, message = res });
                return Json(new { success = true, message = res });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BlockUser(string blockUserId, string blockUserName)
        {
            BlockedUsers model = new BlockedUsers();
            var currentUserId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub").Value;
            var Username = User?.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("preferred_username").Value;
            model.UserId = currentUserId;
            model.BlockUserId = blockUserId;
            model.BlockUserName = blockUserName;
            model.UserName = Username;
            var res = await _mediator.Send(new BlockUserCommand(model));
            if (res != "Successfully Saved")
                return Json(new { success = false, message = res });
                return Json(new { success = true, message = res });
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
