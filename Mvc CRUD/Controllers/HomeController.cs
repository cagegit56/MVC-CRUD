using AutoMapper;
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
using Mvc_CRUD.Dto;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            await AddUser();
            var res = await _context.Post.AsNoTracking().OrderByDescending(x => x.CreatedOn).ToListAsync();
            //var res = await _context.Post.Include(x => x.Comments.OrderByDescending(c => c.SentOn))
            //    .ThenInclude(x => x.Reply.OrderByDescending(x => x.SentOn)).OrderByDescending(x => x.CreatedOn).ToListAsync();
            var CurrentUser = await _mediator.Send(new GetUserProfileQuery(_currentUserId));
            ViewBag.Username = CurrentUser.UserName;
            ViewBag.Lastname = CurrentUser.LastName;
            ViewBag.UserProfilePic = CurrentUser.UserProfilePicUrl;
            return View(res);            
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
            var res = await _mediator.Send(new FriendRequestQuery(pgFilter, _currentUserId, _currentUserName));
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
        public async Task<IActionResult> UpdateUserProfile()
        {
            var res = await _context.Profile.Where(x => x.UserId == _currentUserId).FirstOrDefaultAsync();
            return View(res);
        }

        [HttpPost]
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


        [Authorize]
        public async Task<IActionResult> AddUser()
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                bool existingUser = await _context.ChatUsers.AsNoTracking().AnyAsync(x => x.UserId == _currentUserId);
                var Surname = User.FindFirst(ClaimTypes.Surname)?.Value ?? User.FindFirst("family_name")?.Value;
                if (!existingUser)
                {
                    var model = new Chat_Users
                    {
                        UserName = _currentUserName,
                        FirstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? User.FindFirst("given_name")?.Value!,
                        LastName = Surname,
                        Email = User.FindFirst(ClaimTypes.Email)?.Value!,
                        //Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList()
                    };                    
                    await _context.ChatUsers.AddAsync(model);
                }               

                var existingProfile = await _context.Profile.AsNoTracking().AnyAsync(x => x.UserId == _currentUserId);
                if (!existingProfile)
                {
                    var profileModel = new UserProfile()
                    {
                        UserId = _currentUserId,
                        UserName = _currentUserName,
                        LastName = Surname!
                    };
                    await _context.Profile.AddAsync(profileModel);
                }
               
               if(!existingUser || !existingProfile)
                {
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                    
                return Ok("Saved Successfully");
            }
            catch(Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest($"Unable to save User's Info : {ex.Message}");
            }
        }

        public async Task<IActionResult> Posts()
        {
            var res = await _context.Post.Include(x => x.Comments).OrderByDescending(x => x.CreatedOn).ToListAsync();
            var CurrentUser = await _mediator.Send(new GetUserProfileQuery(_currentUserId));
            ViewBag.Username = CurrentUser.UserName;
            ViewBag.Lastname = CurrentUser.LastName;
            ViewBag.UserProfilePic = CurrentUser.UserProfilePicUrl;
            return View(res);
        }

        [HttpPost]
        public async Task<IActionResult> SendComment(Comments model)
        {
            using var trans = await _context.Database.BeginTransactionAsync();
            try
            {
                model.UserId = _currentUserId;
                model.UserName = _currentUserName;
                var postInfo = await _context.Post.Where(x => x.TotalComments == model.PostId)
                    .ExecuteUpdateAsync(p => p.SetProperty(x => x.TotalComments, c => c.TotalComments + 1));
                await _context.Comment.AddAsync(model);
                await _context.SaveChangesAsync();
                await trans.CommitAsync();
                TempData["Message"] = "SuccessFully Sent";
                return Json(new { success = true, message = "Sent Successfully" });
            }
            catch (Exception ex)
            {
                await trans.RollbackAsync();
                return Json(new { success = true, message = $"Failed due to : {ex.Message}" });
            }
            

        }

        [HttpPost]
        public async Task<IActionResult> SendReplyComment(CommentsReply model)
        {
            await _context.ReplyComments.AddAsync(model);
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Sent Successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> UserPosts()
        {
            var res = await _context.Post.Include(x => x.Comments).Where(x => x.UserId == _currentUserId)
                                         .OrderByDescending(x => x.CreatedOn).AsSplitQuery().ToListAsync();
            return Json(res);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost(string content, string selectedColor, IFormFile postImage, string Scope)
        {
            try
            {
                var model = new Posts();
                var CurrentUser = await _mediator.Send(new GetCurrentUserInfoQuery(_currentUserId));
                model.UserId = CurrentUser.UserId;
                model.UserName = CurrentUser.UserName!;
                model.LastName = CurrentUser.LastName!;
                model.UserImageUrl = CurrentUser.ProfilePicPath;
                model.PostScope = Scope;
                model.Content = content;
                model.PostBgColour = selectedColor;
                if (postImage != null && postImage.Length > 0)
                {
                    string folder = Path.Combine("wwwroot/images/PostPictures");
                    Directory.CreateDirectory(folder);

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(postImage.FileName);
                    string filePath = Path.Combine(folder, fileName);
                    ViewBag.PostPic = fileName;

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await postImage.CopyToAsync(stream);
                    }

                    model.ImageContentUrl = "/images/PostPictures/" + fileName;
                }

                _context.Post.Add(model);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "successful" });
            }
            catch (Exception Ex)
            {
                return Json(new { success = false, message = $"Unsuccessful due to : {Ex.Message}" });
            }
        }

        [HttpPut]
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

            }catch(Exception Ex)
            {
                await transaction.RollbackAsync();
                return Json(new { success = false, message = $"Failed due to : {Ex.Message}" });
            }
         
        }

        [HttpGet]
        public async Task<IActionResult> GetComments(int postId)
        {
            var res = await _context.Comment.Where(x => x.PostId == postId).OrderByDescending(x => x.SentOn).AsNoTracking().ToListAsync();            
            return Json(res);
        }

        [HttpGet]
        public async Task<IActionResult> GetComments2(int postId)
        {
            var res = await _context.Comment.Where(x => x.PostId == postId)
                .Select(x => new CommentsDto
                {
                    Id = x.Id,
                    UserName = x.UserName,
                    LastName = x.LastName,
                    UserImageUrl = x.UserImageUrl,
                    Message = x.Message,
                    PostId = x.PostId,
                    SentOn = x.SentOn,
                    Reply = x.Reply.Select(r => new CommentsReplyDto
                    {
                        Id = r.Id,
                        UserName = r.UserName,
                        LastName = r.LastName,
                        UserImageUrl = r.UserImageUrl,
                        Message = r.Message,
                        SentOn = r.SentOn,
                        CommentId = r.CommentId
                    }).ToList()

                })
                .OrderByDescending(x => x.SentOn).AsSplitQuery().AsNoTracking().ToListAsync();
            //var res = _mapper.Map<List<CommentsDto>>(query);
            return Json(res);

            //var query = await _context.Comment.Include(x => x.Reply).Where(x => x.PostId == postId)
            //.AsSplitQuery().OrderByDescending(x => x.SentOn).AsNoTracking().ToListAsync();
            //var res = _mapper.Map<List<CommentsDto>>(query);
            //return Json(res);
        }

        [HttpPost]
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
