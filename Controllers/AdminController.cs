using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Repository;
using X.PagedList;

namespace WebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDB _context;
        private readonly IConfiguration _appconf;

        public AdminController(ILogger<HomeController> logger, AppDB context, IConfiguration appconf)
        {
            _logger = logger;
            _context = context;
            _appconf = appconf;
        }

        [Authorize]
        [Route("~/Admin/Index", Name = "~/Admin/Index"), Route("~/Admin", Name = "~/Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("~/Admin/Users")]
        [Authorize]
        public async Task<IActionResult> Users(string? filter, int pg = 1)
        {
            if (pg != null && pg < 1)
            {
                pg = 1;
            }

            ViewBag.Filter = filter;

            var pageSize = 10;

            if ((filter == null ? "" : filter.Trim()) == "")
            {
                var qry = _context.account.Include(e => e.Role).AsNoTracking().OrderBy(p => p.username).ToPagedList(pg, pageSize);
                return View("Users", qry);
            }
            else
            {
                var qry = _context.account.Include(e => e.Role).AsNoTracking()
                    .Where(
                        acc =>
                        EF.Functions.Like(acc.username, "%" + filter + "%") ||
                        EF.Functions.Like(acc.namaUser, "%" + filter + "%") ||
                        EF.Functions.Like(acc.email, "%" + filter + "%")
                        )
                    .OrderBy(p => p.username).ToPagedList(pg, pageSize);
                return View("Users", qry);
            }
        }

        [Route("~/Admin/Products")]
        [Authorize]
        public IActionResult Products()
        {
            return View();
        }

        [Route("~/Admin/Scan")]
        [Authorize]
        public IActionResult Scan(string? filter, int pg = 1)
        {
            if (pg != null && pg < 1)
            {
                pg = 1;
            }

            ViewBag.Filter = filter;

            var pageSize = 10;

            if ((filter == null ? "" : filter.Trim()) == "")
            {
                var qry = _context.log_scanning.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                return View("Scan", qry);
            }
            else
            {
                var qry = _context.log_scanning.AsNoTracking()
                    .Where(
                        acc =>
                            EF.Functions.Like(acc.productId, "%" + filter + "%") ||
                            EF.Functions.Like(acc.qrNo, "%" + filter + "%") 
                        )
                    .OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                return View("Scan", qry);
            }
        }

        [Route("~/Admin/Pengaduan")]
        [Authorize]
        public IActionResult Pengaduan(string? filter, int pg = 1)
        {
            if (pg != null && pg < 1)
            {
                pg = 1;
            }

            ViewBag.Filter = filter;

            var pageSize = 10;

            if ((filter == null ? "" : filter.Trim()) == "")
            {
                var qry = _context.report_product.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                return View("Pengaduan", qry);
            }
            else
            {
                var qry = _context.report_product.AsNoTracking()
                    .Where(
                        acc =>
                            EF.Functions.Like(acc.namaLengkap, "%" + filter + "%") ||
                            EF.Functions.Like(acc.email, "%" + filter + "%") ||
                            EF.Functions.Like(acc.descPelapor, "%" + filter + "%") ||
                            EF.Functions.Like(acc.nomorTelp, "%" + filter + "%") ||
                            EF.Functions.Like(acc.namaProduk, "%" + filter + "%")
                        )
                    .OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                return View("Pengaduan", qry);
            }
        }

        [Route("~/Admin/Profile")]
        [Authorize]
        public IActionResult Profile()
        {
            //Get Data User Login
            var dataProfile = new UserProfileDTO();
            dataProfile.Roles = _context.role.Where(e => e.IsActive == 1).ToList();

            var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.Sid) == null ? "0" : User.FindFirstValue(ClaimTypes.Sid));

            if (userId > 0)
            {
                var data = _context.account.Find(userId);
                if (data != null)
                {
                    dataProfile.namaUser = data.namaUser;
                    dataProfile.email = data.email;
                    dataProfile.username = data.username;
                    dataProfile.userId = data.userId;
                    dataProfile.roleId = data.roleId;
                }
            }

            return View(dataProfile);
        }

        [Route("~/Admin/Settings")]
        [Authorize]
        public IActionResult Settings()
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
