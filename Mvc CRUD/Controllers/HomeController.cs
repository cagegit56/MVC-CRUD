using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Mvc_CRUD.CQRS.Commands;
using Mvc_CRUD.CQRS.Queries;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using System.Diagnostics;

namespace Mvc_CRUD.Controllers
{
    public class HomeController : Controller
    {
        private readonly DataDbContext _context;
        private readonly IMediator _mediator;

        public HomeController(ILogger<HomeController> logger, DataDbContext context, IMediator mediator)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _mediator = mediator;
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
            if (!res) return Json(new { success = false, message = "Failed to create a post." });
            return Json(new { success = true, message = "Successfully created a new post." });
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
            if (!string.IsNullOrEmpty(toFriend)) return Json(res);
            return View(res);
        }

        [Authorize]
        public async Task<IActionResult> SendMessage(Chat model)
        {
            var res = await _mediator.Send(new SendMessageCommand(model));
            if (!res) return Json(new { success = false, message = "Failed to send a message." });
            return Json(new { success = true, message = "Successfully Sent." });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendFriendRequest(FriendRequest model)
        {
            var res = await _mediator.Send(new SendFriendRequestCommand(model));
            if (!res) return Json(new { success = false, message = "failed to send a request."});
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
            if (!res) return Json(new { success = false, message = "Failed to add friend" });
            return Json(new { success = true, message = "Successfully Accepted/relationship already exists" });
        }

        [Authorize]
        [HttpPut]
        public async Task<IActionResult> RejectRequest(string friendUserId)
        {
            var res = await _mediator.Send(new RejectRequestCommand(friendUserId));
            if (!res) return Json(new { success = false, message = "Failed to reject." });
            return Json(new { success = true, message = "SuccessFully rejected." });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> BlockUser(BlockedUsers model)
        {
            var res = await _mediator.Send(new BlockUserCommand(model));
            if (!res) return Json(new { success = false, message = "Failed to Block user." });
            return Json(new { success = true, message = "Successfully blocked."});
        }    

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var res = await _mediator.Send(new LikeCommand(postId));
            if (!res) return Json(new {success = false, message = "Failed to Like"});
            return Json(new { success = true, message = "Successfully Liked"});
        }

        [HttpPut]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> UnlikePost(int postId)
        {
            var res = await _mediator.Send(new UnlikePostCommand(postId));
            if (!res) return Json(new { success = false, message = "Failed to unlike."});
            return Json(new { success = true, message = "Successfully unliked."});
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendComment(Comments model)
        {
            var res = await _mediator.Send(new SendCommentCommand(model));
            if (!res) return Json(new { success = false, message = "Failed to send a comment."});
            return Json(new { success = true, message = "Sent Successfully."});

        }

        [HttpPost]
        [Authorize]
        [EnableRateLimiting("RateLimitPolicy")]
        public async Task<IActionResult> SendReplyComment(CommentsReply model)
        {
            var res = await _mediator.Send(new SendReplyCommand(model));
            if (!res) return Json(new { success = false, message = "Failed to send a reply."});
            return Json(new { success = true, message = "Sent Successfully."});
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SendReplyOfReply(ReplyOfReply model)
        {
            var res = await _mediator.Send(new SendReplyOfReplyCommand(model));
            if (!res) return Json(new { success = false, message = "Failed to send a reply of reply."});
            return Json(new { success = true, message = "Sent Successfully."});
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetComments(int postId)
        {
            var res = await _mediator.Send(new GetCommentsQuery(postId));
            return Json(res);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Gallery(IFormFile photo)
        {
            var res = await _mediator.Send(new GalleryCommand(photo));
            if (!res) return Json(new { success = false, message = "Failed to save image." });
            return Json(new { success = true, message = "Successful saved" });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserProfileInfo()
        {
            var res = await _mediator.Send(new GetUserProfileQuery());
            return Json(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserProfile()
        {
            var res = await _mediator.Send(new GetUserProfileQuery());
            return View(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile()
        {
            var res = await _mediator.Send(new GetUserProfileQuery());
            return View(res);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateUserProfile(UserProfile model)
        {
            var res = await _mediator.Send(new UpdateUserProfileCommand(model));
            if (!res) return RedirectToAction("UpdateUserProfile");
            //TempData["Message"] = "SuccessFully Updated";                
            //TempData["Message"] = $"Failed to update user info due to : {Ex.Message} ";
            return RedirectToAction("UpdateUserProfile");
            
        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateProfilePicture(IFormFile profileImage)
        {
            var res = await _mediator.Send(new UpdateProfilePictureCommand(profileImage));
            if (!res) return Json(new { success = false, message = "Failed to update profile picture."});
            return Json(new { success = true, message = "Successful updated profile picture"});
                

        }

        [HttpPatch]
        [Authorize]
        public async Task<IActionResult> UpdateCoverPicture(IFormFile coverImage)
        {
            var res = await _mediator.Send(new UpdateCoverPictureCommand(coverImage));
            if (!res) return Json(new { success = true, message = "Failed to update cover image."});
            return Json(new { success = true, message = "Sucessfully updated cover image."});

        } 

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> UserPosts()
        {
            var res = await _mediator.Send(new GetCurrentUserPostsQuery());
            return Json(res);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserPostComments(int postId)
        {
            var res = await _mediator.Send(new GetCurrentUserPostCommentsQuery(postId));
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
            if (rec != null)
                _context.Chats.Remove(rec);
            await _context.SaveChangesAsync();
            TempData["Message"] = "Deleted record Successfully!";
            return Json(new { success = true, message = "Deleted successfully", id = Id });
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
