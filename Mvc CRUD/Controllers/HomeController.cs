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

        public HomeController(ILogger<HomeController> logger, DataDbContext context, IGetAllService getService,
            IMemoryCache cache, IPaginationService pagination, IMediator mediator, IHttpContextAccessor httpContext)
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
        public async Task<IActionResult> Chats(string toFriend)
        {
            var res = await _mediator.Send(new GetChatsQuery(_currentUserId, _currentUserName, toFriend));
            if (!string.IsNullOrEmpty(toFriend))
                return Json(res);
                return View(res);
        }

        [Authorize]
        public async Task<IActionResult> SendDbMessage(string username, string friend, string message)
        {
            var model = new Chat();
            model.UserName = username;
            model.ToUser = friend;
            model.Message = message;
            var res = await _mediator.Send(new SendMessageCommand(model));
            return Json("Success");
        }

        [Authorize]
        public async Task<IActionResult> request(string toUserId, string toUserName, string email)
        {
            var model = new FriendRequest();
            model.UserId = _currentUserId;
            model.UserName = _currentUserName;
            model.ToUserId = toUserId;
            model.ToUserName = toUserName;
            model.ToUserEmail = email;
            var res = await _mediator.Send(new SendFriendRequestCommand(model));
            if (res != "SuccessFully Sent")
                return Json(new { success = true, message = $"Failed to send a friend request due to : {res}" });

            return Json(new { success = true, message = "Friend Request Successfully Sent" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllSentRequest(PaginationFilter filter)
        {
            var res = await _mediator.Send(new GetAllSentRequestQuery(filter, _currentUserId));
            return Json(res);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> friendRequests([FromQuery] PaginationFilter pgFilter)
        {
            var res = await _mediator.Send(new FriendRequestQuery(pgFilter, _currentUserId));
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
            return View(res);
        }

        [Authorize]
        public async Task<IActionResult> RecievedFriendRequest(PaginationFilter filter)
        {
            var res = await _mediator.Send(new ReceivedFriendRequestQuery(filter, _currentUserId));
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                Response.ContentType = "application/json";
                return Json(new
                {
                    data = res.Data,
                    totalRecords = res.TotalRecords,
                    totalPages = res.TotalPages,
                    CurrentPage = filter.PageNumber,
                    pageSize = filter.PageSize
                });
            }
            return View(res);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AcceptRequest(string userId, string frndName)
        {
            var model = new Friends();
            model.UserId = _currentUserId;
            model.UserName = _currentUserName;
            model.FriendId = userId;
            model.FriendName = frndName;
            var res = await _mediator.Send(new AddFriendCommand(model));
            if (res != "Friend Request Accepted.")
                return Json(new { success = false, message = $"Failed to add friend due to : {res}" });
            return Json(new { success = true, message = res });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> RejectRequest(string friendUserId)
        {
            var res = await _mediator.Send(new RejectRequestCommand(_currentUserId, friendUserId));
            if (res != "Request Cancelled Succesfully.")
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = res });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BlockUser(string blockUserId, string blockUserName)
        {
            BlockedUsers model = new BlockedUsers();
            model.UserId = _currentUserId;
            model.BlockUserId = blockUserId;
            model.BlockUserName = blockUserName;
            model.UserName = _currentUserName;
            var res = await _mediator.Send(new BlockUserCommand(model));
            if (res != "Successfully Saved")
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = res });
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
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            var firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value;
            var lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value;
            ViewBag.FirstName = firstName;
            ViewBag.LastName = lastName;
            ViewBag.UserId = _currentUserId;
            ViewBag.Username = _currentUserName;
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
