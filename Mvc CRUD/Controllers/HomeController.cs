using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.CQRS.Commands;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Formats.Asn1.AsnWriter;
using static System.Net.Mime.MediaTypeNames;

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
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContext;
        private string _currentUserId;
        private string _currentUserName;

        public HomeController(ILogger<HomeController> logger, DataDbContext context, IGetAllService getService,
            IMemoryCache cache, IPaginationService pagination, IMediator mediator, IMapper mapper, IHttpContextAccessor httpContext)
        {
            _logger = logger;
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _getService = getService ?? throw new ArgumentNullException(nameof(getService));
            _pagination = pagination ?? throw new ArgumentNullException(nameof(pagination));
            _httpContext = httpContext ?? throw new ArgumentNullException(nameof(httpContext));
            _cache = cache;
            _mediator = mediator;
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _currentUserId = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? _httpContext.HttpContext.User.FindFirst("sub").Value;
            _currentUserName = _httpContext.HttpContext?.User.FindFirst(ClaimTypes.Name)?.Value ?? _httpContext.HttpContext.User.FindFirst("preferred_username").Value;
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Index([FromQuery] PaginationFilter pgFilter, string filter)
        {
            var res = await _mediator.Send(new GetAllPostsQuery());          
            return View(res);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePost(Posts model, IFormFile postImage)
        {
            var res = await _mediator.Send(new CreatePostCommand(model,postImage));
            if (!res)
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = "Successfully created a new post" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AdminPortal([FromQuery] PaginationFilter pgFilter, string filter)
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
            return View(res);
        }
    

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Chats(string toFriend)
        {
            var res = await _mediator.Send(new GetChatsQuery(toFriend));
            if (!string.IsNullOrEmpty(toFriend))
                return Json(res);
            return View(res);
        }

        [Authorize]
        public async Task<IActionResult> SendMessage(Chat model)
        {
            var res = await _mediator.Send(new SendMessageCommand(model));
            if (!res)
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = res });
        }

        [Authorize]
        public async Task<IActionResult> SendFriendRequest(FriendRequest model)
        {
            var res = await _mediator.Send(new SendFriendRequestCommand(model));
            if (!res)
                return Json(new { success = false, message = $"Failed to send a friend request due to : {res}" });
            return Json(new { success = true, message = "Friend Request Successfully Sent" });
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllSentRequest(PaginationFilter filter)
        {
            var res = await _mediator.Send(new GetAllSentRequestQuery(filter));
            return Json(res);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> friendRequests([FromQuery] PaginationFilter pgFilter)
        {
            var res = await _mediator.Send(new FriendRequestQuery(pgFilter));
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
        [HttpGet]
        public async Task<IActionResult> RecievedFriendRequest(PaginationFilter filter)
        {
            var res = await _mediator.Send(new ReceivedFriendRequestQuery(filter));
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
        public async Task<IActionResult> AcceptRequest(Friends model)
        {
            var res = await _mediator.Send(new AddFriendCommand(model));
            if (!res)
                return Json(new { success = false, message = $"Failed to add friend due to : {res}" });
            return Json(new { success = true, message = "Successfully Accepted/relationship already exists" });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> RejectRequest(string friendUserId)
        {
            var res = await _mediator.Send(new RejectRequestCommand(friendUserId));
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

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var res = await _mediator.Send(new LikeCommand(postId));
            if(res)
                return Json(new {success = true, message = "Liked successfully"});
            return Json(new { success = false, message = $"Unsuccessfully, due to {res}" });
        }

        [HttpPut]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var res = await _mediator.Send(new UnlikePostCommand(postId, _currentUserName));
            if(res)
                return Json(new { success = true, message = "Unliked Successfully" });
            return Json(new { success = false, message = $"failed to unlike due to {res}" });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendComment(Comments model)
        {
            var res = await _mediator.Send(new SendCommentCommand(model));
            if (!res)
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = "Sent Successfully" });

        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> SendReplyComment(CommentsReply model)
        {
            var res = await _mediator.Send(new SendReplyCommand(model));
            if (!res)
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = "Sent Successfully" });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendReplyOfReply(ReplyOfReply model)
        {
            var res = await _mediator.Send(new SendReplyOfReplyCommand(model));
            if (!res)
                return Json(new { success = false, message = res });
            return Json(new { success = true, message = "Sent Successfully" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetComments(int postId)
        {
            var res = await _mediator.Send(new GetCommentsQuery(postId));
            return Json(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserInfo()
        {
            var res = await _mediator.Send(new GetUserProfileQuery());
            return Json(res);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> AddData(int? Id)
        {
            if (Id.HasValue && Id.Value > 0)
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
        public async Task<IActionResult> UserProfile()
        {
            var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
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
            return View(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile()
        {
            var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
            return View(res);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(UserProfile userInfo)
        {
            try
            {
                var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
                if (res == null)
                {
                    TempData["Message"] = "User not found";
                    return RedirectToAction("UpdateUserProfile");
                }

                if (string.IsNullOrWhiteSpace(userInfo.Bio))
                    userInfo.Bio = res.Bio;
                if (string.IsNullOrWhiteSpace(userInfo.Location))
                    userInfo.Location = res.Location;
                if (string.IsNullOrWhiteSpace(userInfo.HighSchoolName))
                    userInfo.HighSchoolName = res.HighSchoolName;
                if (string.IsNullOrWhiteSpace(userInfo.Subject))
                    userInfo.Subject = res.Subject;
                if (string.IsNullOrWhiteSpace(userInfo.SchoolPeriod))
                    userInfo.SchoolPeriod = res.SchoolPeriod;
                if (string.IsNullOrWhiteSpace(userInfo.CollegeName))
                    userInfo.CollegeName = res.CollegeName;
                if (string.IsNullOrWhiteSpace(userInfo.Course))
                    userInfo.Course = res.Course;
                if (string.IsNullOrWhiteSpace(userInfo.CollegePeriod))
                    userInfo.CollegePeriod = res.CollegePeriod;
                if (string.IsNullOrWhiteSpace(userInfo.RelationShipStatus))
                    userInfo.RelationShipStatus = res.RelationShipStatus;
                if (string.IsNullOrWhiteSpace(userInfo.JobTitle))
                    userInfo.JobTitle = res.JobTitle;
                if (string.IsNullOrWhiteSpace(userInfo.Industry))
                    userInfo.Industry = res.Industry;
                if (string.IsNullOrWhiteSpace(userInfo.JobPeriod))
                    userInfo.JobPeriod = res.JobPeriod;
                if (string.IsNullOrWhiteSpace(userInfo.FromLocation))
                    userInfo.FromLocation = res.Bio;
                if (string.IsNullOrWhiteSpace(userInfo.Website))
                    userInfo.Website = res.Website;

                res.Bio = userInfo.Bio;
                res.Location = userInfo.Location;
                res.HighSchoolName = userInfo.HighSchoolName;
                res.Subject = userInfo.Subject;
                res.SchoolPeriod = userInfo.SchoolPeriod;
                res.CollegeName = userInfo.CollegeName;
                res.Course = userInfo.Course;
                res.CollegePeriod = userInfo.CollegePeriod;
                res.RelationShipStatus = userInfo.RelationShipStatus;
                res.JobTitle = userInfo.JobTitle;
                res.Industry = userInfo.Industry;
                res.FromLocation = userInfo.FromLocation;
                res.Website = userInfo.Website;
                res.JobPeriod = userInfo.JobPeriod;

                _context.Profile.Update(res);
                await _context.SaveChangesAsync();
                TempData["Message"] = "SuccessFully Updated";
                return RedirectToAction("UpdateUserProfile");
            }
            catch (Exception Ex)
            {
                TempData["Message"] = $"Failed to update user info due to : {Ex.Message} ";
                return RedirectToAction("UpdateUserProfile");
            }

        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(IFormFile profileImage)
        {
            if (profileImage.Length > 0 && profileImage != null)
            {
                var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
                if (res != null)
                {
                    string folder = Path.Combine("wwwroot/images/ProfilePictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    res.UserProfilePicUrl = "/images/ProfilePictures/" + fileName;
                    _context.Profile.Update(res);
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true, message = "Successful" });
            }
            else
            {
                return Json(new { success = false, message = $"Failed due to " });
            }

        }

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdateCoverPicture(IFormFile coverImage)
        {
            if (coverImage.Length > 0 && coverImage != null)
            {
                var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
                if (res != null)
                {
                    string folder = Path.Combine("wwwroot/images/CoverPictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(coverImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }

                    res.UserCoverPicUrl = "/images/CoverPictures/" + fileName;
                    _context.Profile.Update(res);
                    await _context.SaveChangesAsync();
                }
                return Json(new { success = true, message = "Successful" });
            }
            else
            {
                return Json(new { success = false, message = $"Failed due to " });
            }

        }



        //*************** old posts
        //[Authorize]
        //public async Task<IActionResult> Posts()
        //{
        //    var res = await _context.Post.Include(x => x.Comments).OrderByDescending(x => x.CreatedOn).ToListAsync();
        //    var CurrentUser = await _mediator.Send(new GetUserProfileQuery(_currentUserId));
        //    ViewBag.Username = CurrentUser.UserName;
        //    ViewBag.Lastname = CurrentUser.LastName;
        //    ViewBag.UserProfilePic = CurrentUser.UserProfilePicUrl;
        //    return View(res);
        //}      


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserPosts()
        {
            var res = await _context.Post.Where(x => x.UserId == _currentUserId)
                                       .OrderByDescending(x => x.CreatedOn).ToListAsync();
            //var res = await _context.Post.Include(x => x.Comments).Where(x => x.UserId == _currentUserId)
            //                             .OrderByDescending(x => x.CreatedOn).AsSplitQuery().ToListAsync();
            return Json(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPostComments(int postId)
        {
            var res = await _context.Comment.Where(x => x.PostId == postId).OrderByDescending(x => x.SentOn).AsNoTracking().ToListAsync();
            return Json(res);
        }
   

        [HttpPut]
        [Authorize]
        public async Task<IActionResult> ChangeProfilePic(IFormFile profileImage)
        {
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                if (profileImage != null && profileImage.Length > 0)
                {
                    var rec = await _context.ChatUsers.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();

                    string folder = Path.Combine("wwwroot/images/ProfilePictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(profileImage.FileName);
                    string filePath = Path.Combine(folder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    rec!.ProfilePicPath = "/images/ProfilePictures/" + fileName;
                    _context.ChatUsers.Update(rec);

                    var postPic = await _context.Post.Where(x => x.UserId == _currentUserId).ToListAsync();
                    var model = new List<Posts>();
                    foreach (var post in postPic)
                    {
                        post.UserImageUrl = $"/images/ProfilePictures/{fileName}";
                        model.Add(post);
                    }
                    _context.Post.UpdateRange(model);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }

                return Json(new { success = true, message = "successful" });

            }
            catch (Exception Ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Failed due to : {Ex.Message}" });
            }

        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Gallery(IFormFile photo)
        {
            if (photo != null && photo.Length > 0)
            {
                string folder = Path.Combine("wwwroot/images/Gallery");
                Directory.CreateDirectory(folder);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(photo.FileName);
                string filePath = Path.Combine(folder, fileName);
                ViewBag.PostPic = fileName;

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }

                //model.ImageContentUrl = "/images/Gallery/" + fileName;
            }

            //_context.Post.Add(model);
            //await _context.SaveChangesAsync();
            return Json(new { success = true, message = "successful" });
        }

        //var accessToken = await HttpContext.GetTokenAsync("access_token");
        //Console.WriteLine(accessToken);

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

        [Authorize]
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
