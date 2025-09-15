using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Mvc_CRUD.Common;
using Mvc_CRUD.Models;
using Mvc_CRUD.Pagination;
using Mvc_CRUD.Services;

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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] PaginationFilter pgFilter, string? filter)
        {
            try { 
            string cacheKey = "cacheAll";
            if (!_cache.TryGetValue(cacheKey, out List<Chat>? res))
            {
                res = await _context.Chats.AsNoTracking().ToListAsync();
                _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
            }

            IEnumerable<Chat>? queryData = res;
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                queryData = queryData.Where(x => x.UserName.Contains(filter) ||
                    x.ToUser.Contains(filter) ||
                    x.Message.Contains(filter));
            }

            var paginatedRes = await _pagination.Paginate(queryData.ToList(), pgFilter);

            return Json(new
            {
                Data = paginatedRes.Data,
                TotalRecords = paginatedRes.TotalRecords,
                TotalPages = paginatedRes.TotalPages,
                CurrentPage = pgFilter.PageNumber,
                PageSize = pgFilter.PageSize
            });
                }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message
            });
            }
        }

        //[HttpGet]
        //public async Task<IActionResult> Index([FromQuery] PaginationFilter pgFilter, string? filter)
        //{
        //    string cacheKey = "cacheAll";
        //    if (!_cache.TryGetValue(cacheKey, out List<Chat>? res))
        //    {
        //        res = await _context.Chats.AsNoTracking().ToListAsync();
        //        _cache.Set(cacheKey, res, TimeSpan.FromMinutes(10));
        //    }

        //    IEnumerable<Chat>? queryData = res;
        //    if (!string.IsNullOrEmpty(filter))
        //    {
        //        filter = filter.ToLower();
        //        queryData = queryData.Where(x => x.UserName.Contains(filter) ||
        //            x.ToUser.Contains(filter) ||
        //            x.Message.Contains(filter));
        //    }

        //    var paginatedRes = await _pagination.Paginate(queryData.ToList(), pgFilter);
        //    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        //    {
        //        return Json(new
        //        {
        //            data = paginatedRes.Data,      
        //            pageNumber = paginatedRes.PageNumber,
        //            pageSize = paginatedRes.PageSize,
        //            totalRecords = paginatedRes.TotalRecords,
        //            totalPages = paginatedRes.TotalPages
        //        });
        //    }
        //    return View(paginatedRes);
        //}

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
            return RedirectToAction("Index");
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
