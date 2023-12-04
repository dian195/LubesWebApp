using iTextSharp.text;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Ocsp;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Repository;
using WebApp.Repository.Interfaces;
using WebApp.Repository.Services;
using X.PagedList;

namespace WebApp.Controllers
{
    public class AdminController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDB _context;
        private readonly IConfiguration _appconf; 
        private readonly IExport _export; 

        public AdminController(ILogger<HomeController> logger, AppDB context, IConfiguration appconf, IExport export)
        {
            _logger = logger;
            _context = context;
            _appconf = appconf;
            _export = export;
        }

        [Authorize]
        [Route("~/Admin/Index", Name = "~/Admin/Index"), Route("~/Admin", Name = "~/Admin")]
        public IActionResult Index()
        {
            return View();
        }

        #region Users

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

        [HttpGet]
        [Route("~/Admin/NewUser")]
        [Authorize]
        public IActionResult NewUser()
        {
            //Reset Data
            NewUserDTO rv = new NewUserDTO();
            rv.Roles = _context.role.Where(e => e.IsActive == 1).ToList();
            rv.roleId = 0;
            rv.username = "";
            rv.password = "";
            rv.confPassword = "";
            rv.email = "";
            rv.namaUser = "";
            return PartialView("_AddUser", rv);
        }

        [HttpGet]
        [Route("~/Admin/EditUser")]
        [Authorize]
        public IActionResult EditUser(int id)
        {
            //Reset Data
            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            EditUserDTO rv = new EditUserDTO();
            rv.Roles = _context.role.Where(e => e.IsActive == 1).ToList();
            rv.usrId = id;
            rv.roleId = 0;
            rv.isActive = 0;
            rv.username = "";
            rv.email = "";
            rv.namaUser = "";

            var data = _context.account.Find(id);
            if (data != null)
            {
                rv.namaUser = data.namaUser;
                rv.email = data.email;
                rv.username = data.username;
                rv.isActive = data.isActive;
                rv.usrId = data.userId;
                rv.roleId = data.roleId;
            }
            else
            {
                ViewData["errormessage"] = "User tidak ditemukan !";
                return PartialView("_EditUser", rv);
            }
            
            return PartialView("_EditUser", rv);
        }

        [HttpGet]
        [Route("~/Admin/ChangePasswordUser")]
        [Authorize]
        public IActionResult ChangePasswordUser(int id)
        {
            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            //Reset Data
            UpdatePassProfileDTO rv = new UpdatePassProfileDTO();
            rv.usrId = id;
            rv.oldpass = "";
            rv.newpass = "";
            rv.confirmpass = "";

            var data = _context.account.Where(e => e.userId == id).ToList();
            if (data != null)
            {
                return PartialView("_ChangePasswordUser", rv);
            }
            else
            {
                ViewData["errormessage"] = "User tidak ditemukan !";
                return PartialView("_ChangePasswordUser", rv);
            }
        }

        [HttpPost]
        [Route("~/Admin/ChangePasswordUser")]
        [Authorize]
        public IActionResult ChangePasswordUser(UpdatePassProfileDTO reg)
        {
            var gf = new GlobalFunction();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();

            //Validasi
            if (string.IsNullOrEmpty(reg.oldpass) || string.IsNullOrEmpty(reg.confirmpass) || string.IsNullOrEmpty(reg.newpass))
            {
                ModelState.AddModelError("", "Lengkapi data!");
                ViewData["errormessage"] = "Lengkapi data !";
                return PartialView("_ChangePasswordUser", reg);
            }
            if (reg.confirmpass != reg.newpass)
            {
                ModelState.AddModelError("", "Konfirmasi Password tidak sama!");
                ViewData["errormessage"] = "Konfirmasi Password tidak sama !";
                return PartialView("_ChangePasswordUser", reg);
            }
            //End Validasi

            try
            {
                string encOldPassword = EncryptionHelper.Encrypt(reg.oldpass);
                string encNewPassword = EncryptionHelper.Encrypt(reg.newpass);

                var dtusr2 = _context.account.Where(e => e.userId == reg.usrId && e.password == encOldPassword).FirstOrDefault();
                if (dtusr2 == null)
                {
                    return Ok(new { Id = 0, Message = "Password tidak sesuai", reg });
                }

                var dtusr = _context.account.Find(reg.usrId);
                if (dtusr != null)
                {
                    dtusr.password = encNewPassword;
                    dtusr.lastUpdate = DateTime.Now;
                    dtusr.lastUpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.SaveChanges();

                    ViewData["SuccessMessage"] = "Data berhasil disimpan";
                    return PartialView("_ChangePasswordUser", reg);
                }
                else
                {
                    ModelState.AddModelError("", "Data user tidak ditemukan!");
                    ViewData["ErrorMessage"] = "Data user tidak ditemukan!";
                    return PartialView("_ChangePasswordUser", reg);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewData["ErrorMessage"] = ex.Message;
                return PartialView("_ChangePasswordUser", reg);
            }
        }

        [HttpPost]
        [Route("~/Admin/EditUser")]
        [Authorize]
        public IActionResult EditUser(EditUserDTO reg)
        {
            var gf = new GlobalFunction();
            reg.Roles = _context.role.Where(e => e.IsActive == 1).ToList();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();

            //Validasi
            if (reg.roleId == 0 || string.IsNullOrEmpty(reg.namaUser) || string.IsNullOrEmpty(reg.username) || string.IsNullOrEmpty(reg.email))
            {
                ModelState.AddModelError("", "Lengkapi data!");
                ViewData["errormessage"] = "Lengkapi data !";
                return PartialView("_EditUser", reg);
            }

            bool validEmail = gf.ValidEmailDataAnnotations(reg.email == null ? "" : reg.email);
            if (!validEmail)
            {
                ModelState.AddModelError("", "Format email tidak valid !");
                ViewData["errormessage"] = "Format email tidak valid !";
                return PartialView("_EditUser", reg);
            }

            var cekEmail = _context.account.Where(e => e.email == reg.email && e.userId != reg.usrId).ToList();
            if (cekEmail.Count > 0)
            {
                ModelState.AddModelError("", "Email sudah digunakan !");
                ViewData["errormessage"] = "Email sudah digunakan !";
                return PartialView("_EditUser", reg);
            }
            //End Validasi

            try
            {
                var dtusr = _context.account.Find(reg.usrId);
                if (dtusr != null)
                {
                    dtusr.namaUser = reg.namaUser;
                    dtusr.roleId = reg.roleId;
                    dtusr.email = reg.email;
                    dtusr.username = reg.username;
                    dtusr.isActive = reg.isActive;
                    dtusr.lastUpdate = DateTime.Now;
                    dtusr.lastUpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.SaveChanges();

                    ViewData["SuccessMessage"] = "Data berhasil disimpan";
                    return PartialView("_EditUser", reg);
                }
                else
                {
                    ModelState.AddModelError("", "Data user tidak ditemukan!");
                    ViewData["ErrorMessage"] = "Data user tidak ditemukan!";
                    return PartialView("_EditUser", reg);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                ViewData["ErrorMessage"] = ex.Message;
                return PartialView("_EditUser", reg);
            }
        }

        [HttpPost]
        [Route("~/Admin/NewUser")]
        [Authorize]
        public IActionResult NewUser(NewUserDTO reg)
        {
            var data = new UserDTO();
            var gf = new GlobalFunction();
            reg.Roles = _context.role.Where(e => e.IsActive == 1).ToList();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();

            //Validasi
            if (reg.roleId == 0 || string.IsNullOrEmpty(reg.namaUser) || string.IsNullOrEmpty(reg.username) || string.IsNullOrEmpty(reg.email) || string.IsNullOrEmpty(reg.password))
            {
                ModelState.AddModelError("", "Lengkapi data!");
                ViewData["errormessage"] = "Lengkapi data !";
                return PartialView("_AddUser", reg);
            }

            if (reg.password.Trim() != reg.confPassword.Trim())
            {
                reg.password = "";
                reg.confPassword = "";

                ModelState.AddModelError("", "Password tidak sama !");
                ViewData["errormessage"] = "Password tidak sama !";
                return PartialView("_AddUser", reg);
            }

            bool validEmail = gf.ValidEmailDataAnnotations(reg.email == null ? "" : reg.email);
            if (!validEmail)
            {
                ModelState.AddModelError("", "Format email tidak valid !");
                ViewData["errormessage"] = "Format email tidak valid !";
                return PartialView("_AddUser", reg);
            }

            var cekUsername = _context.account.Where(e => e.username == reg.username).ToList();
            if (cekUsername.Count > 0)
            {
                ModelState.AddModelError("", "Username sudah digunakan !");
                ViewData["errormessage"] = "Username sudah digunakan !";
                return PartialView("_AddUser", reg);
            }

            var cekEmail = _context.account.Where(e => e.email == reg.email).ToList();
            if (cekEmail.Count > 0)
            {
                ModelState.AddModelError("", "Email sudah digunakan !");
                ViewData["errormessage"] = "Email sudah digunakan !";
                return PartialView("_AddUser", reg);
            }
            //End Validasi

            data.username = reg.username;
            data.namaUser = reg.namaUser;
            data.roleId = reg.roleId;
            data.password = reg.password;
            data.email = reg.email;
            data.isActive = 1;
            data.createdBy = userId;
            data.createdDate = DateTime.Now;

            try
            {
                _context.account.Add(data);
                _context.SaveChanges();

                reg.username = "";
                reg.namaUser = "";
                reg.email = "";
                reg.password = "";
                reg.confPassword = "";
                reg.roleId = 0;
                
                ViewData["SuccessMessage"] = "Data berhasil disimpan";
                return PartialView("_AddUser", reg);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "ex.Message");
                ViewData["ErrorMessage"] = ex.Message;
                return PartialView("_AddUser", reg);
            }
        }
        #endregion

        #region Products
        [Route("~/Admin/Products")]
        [Authorize]
        public IActionResult Products(string? filter, int pg = 1)
        {
            if (pg != null && pg < 1)
            {
                pg = 1;
            }

            ViewBag.Filter = filter;

            var pageSize = 10;

            if ((filter == null ? "" : filter.Trim()) == "")
            {
                var qry = _context.series_master.AsNoTracking().OrderBy(p => p.seriesID).ToPagedList(pg, pageSize);
                return View("Products", qry);
            }
            else
            {
                var qry = _context.series_master.AsNoTracking()
                    .Where(
                        acc =>
                        EF.Functions.Like(acc.seriesID, "%" + filter + "%") ||
                        EF.Functions.Like(acc.productName, "%" + filter + "%") ||
                        EF.Functions.Like(acc.productPackaging, "%" + filter + "%") ||
                        EF.Functions.Like(acc.productVolume, "%" + filter + "%") 
                        )
                    .OrderBy(p => p.seriesID).ToPagedList(pg, pageSize);
                return View("Products", qry);
            }
        }

        [HttpGet]
        [Route("~/Admin/NewProduct")]
        [Authorize]
        public IActionResult NewProduct()
        {
            //Reset Data
            AddSeriesMasterDTO rv = new AddSeriesMasterDTO();
            rv.productName = rv.seriesId = rv.productVolume = "";
            rv.productPackaging = "0";
            return PartialView("_AddProduct", rv);
        }

        [HttpPost]
        [Route("~/Admin/NewProduct")]
        [Authorize]
        public IActionResult NewProduct(AddSeriesMasterDTO reg)
        {
            var data = new SeriesMasterDTO();
            var gf = new GlobalFunction();
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();

            //Validasi
            if (string.IsNullOrEmpty(reg.productName) || string.IsNullOrEmpty(reg.productPackaging) || string.IsNullOrEmpty(reg.productVolume) || string.IsNullOrEmpty(reg.seriesId))
            {
                ModelState.AddModelError("", "Lengkapi data!");
                ViewData["errormessage"] = "Lengkapi data !";
                return PartialView("_AddProduct", reg);
            }

            if (reg.productPackaging.Trim() == "0")
            {
                ModelState.AddModelError("", "Pilih Packaging !");
                ViewData["errormessage"] = "Pilih Packaging !";
                return PartialView("_AddProduct", reg);
            }

            var cek = _context.series_master.Where(e => e.seriesID == reg.seriesId).ToList();
            if (cek.Count > 0)
            {
                ModelState.AddModelError("", "Series ID sudah digunakan !");
                ViewData["errormessage"] = "Series ID sudah digunakan !";
                return PartialView("_AddProduct", reg);
            }

            //End Validasi

            data.productVolume = reg.productVolume;
            data.productPackaging = reg.productPackaging;
            data.productName = reg.productName;
            data.seriesID = reg.seriesId;
            data.createdBy = userId;
            data.createdDate = DateTime.Now;

            try
            {
                _context.series_master.Add(data);
                _context.SaveChanges();

                reg = new AddSeriesMasterDTO();
                reg.productVolume = "";
                reg.productName = "";
                reg.productPackaging = "0";
                reg.seriesId = "";

                ViewData["SuccessMessage"] = "Data berhasil disimpan";
                return PartialView("_AddProduct", reg);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "ex.Message");
                ViewData["ErrorMessage"] = ex.Message;
                return PartialView("_AddProduct", reg);
            }
        }

        [HttpGet]
        [Route("~/Admin/EditProduct")]
        [Authorize]
        public IActionResult EditProduct(int id)
        {
            //Reset Data
            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();
            EditSeriesMasterDTO rv = new EditSeriesMasterDTO();
            rv.productName = rv.seriesId = rv.productVolume = "";
            rv.productPackaging = "0";
            rv.id = id;

            var data = _context.series_master.Find(id);
            if (data != null)
            {
                rv.productName = data.productName;
                rv.seriesId = data.seriesID;
                rv.productVolume = data.productVolume;
                rv.productPackaging = data.productPackaging;
            }
            else
            {
                ViewData["errormessage"] = "Produk tidak ditemukan !";
                return PartialView("_EditProduct", rv);
            }

            return PartialView("_EditProduct", rv);
        }

        [HttpPost]
        [Route("~/Admin/EditProduct")]
        [Authorize]
        public IActionResult EditProduct(EditSeriesMasterDTO reg)
        {
            string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ViewData["errormessage"] = "";
            ViewData["SuccessMessage"] = "";

            ModelState.Clear();

            //Validasi
            if (reg.id == 0)
            {
                ModelState.AddModelError("", "Data tidak ditemukan !");
                ViewData["errormessage"] = "Data tidak ditemukan !";
                return PartialView("_EditProduct", reg);
            }

            if (string.IsNullOrEmpty(reg.productName) || string.IsNullOrEmpty(reg.productPackaging) || string.IsNullOrEmpty(reg.productVolume) || string.IsNullOrEmpty(reg.seriesId))
            {
                ModelState.AddModelError("", "Lengkapi data!");
                ViewData["errormessage"] = "Lengkapi data !";
                return PartialView("_EditProduct", reg);
            }

            if (reg.productPackaging.Trim() == "0")
            {
                ModelState.AddModelError("", "Pilih Packaging !");
                ViewData["errormessage"] = "Pilih Packaging !";
                return PartialView("_EditProduct", reg);
            }

            var cek = _context.series_master.Where(e => e.seriesID == reg.seriesId && e.id != reg.id).ToList();
            if (cek.Count > 0)
            {
                ModelState.AddModelError("", "Series ID sudah digunakan !");
                ViewData["errormessage"] = "Series ID sudah digunakan !";
                return PartialView("_EditProduct", reg);
            }

            //End Validasi

            try
            {
                var dtusr = _context.series_master.Find(reg.id);
                if (dtusr != null)
                {
                    dtusr.productVolume = reg.productVolume;
                    dtusr.productPackaging = reg.productPackaging;
                    dtusr.productName = reg.productName;
                    dtusr.seriesID = reg.seriesId;
                    dtusr.lastUpdate = DateTime.Now;
                    dtusr.lastUpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.SaveChanges();

                    ViewData["SuccessMessage"] = "Data berhasil disimpan";
                    return PartialView("_EditProduct", reg);
                }
                else
                {
                    ModelState.AddModelError("", "Data tidak ditemukan!");
                    ViewData["ErrorMessage"] = "Data tidak ditemukan!";
                    return PartialView("_EditProduct", reg);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "ex.Message");
                ViewData["ErrorMessage"] = ex.Message;
                return PartialView("_EditProduct", reg);
            }
        }

        #endregion

        #region Scan
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
        #endregion

        #region Report
        [Route("~/Admin/Pengaduan")]
        [Authorize]
        public IActionResult Pengaduan(string? filter, string? fromDate, string? toDate, int pg = 1)
        {
            if (pg != null && pg < 1)
            {
                pg = 1;
            }

            fromDate = fromDate == null ? "" : fromDate;
            toDate = toDate == null ? "" : toDate;

            ViewBag.Filter = filter;
            ViewBag.FromDate = fromDate;
            ViewBag.ToDate = toDate;

            DateTime dtFrom = DateTime.Now;
            DateTime dtTo = DateTime.Now;

            try
            {
                dtFrom = DateTime.Now;
                dtFrom = fromDate.Trim() == "" ? DateTime.Now : DateTime.Parse(fromDate.Trim());
                dtTo = toDate == null ? DateTime.Now : DateTime.Parse(toDate.Trim()).AddDays(1);
                fromDate = fromDate.Trim() == "" ? "" : DateTime.Parse(fromDate.Trim()).ToString("yyyy-MM-dd");
                toDate = toDate.Trim() == "" ? "" : DateTime.Parse(toDate.Trim()).ToString("yyyy-MM-dd");
            }
            catch
            {
                fromDate = "";
                toDate = "";
            }

            var pageSize = 10;

            if ((filter == null ? "" : filter.Trim()) == "")
            {
                if (fromDate.Trim() != "" && toDate.Trim() != "")
                {
                    var qry = _context.report_product.AsNoTracking()
                    .Where(
                        acc =>
                            acc.CreatedAt >= dtFrom && acc.CreatedAt < dtTo
                        )
                    .OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                    return View("Pengaduan", qry);
                }
                else
                {
                    var qry = _context.report_product.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
                    return View("Pengaduan", qry);
                }
            }
            else
            {
                if (fromDate.Trim() != "" && toDate.Trim() != "")
                {
                    var qry = _context.report_product.AsNoTracking()
                    .Where(
                        acc =>
                            acc.CreatedAt >= dtFrom && acc.CreatedAt < dtTo &&
                            (EF.Functions.Like(acc.namaLengkap, "%" + filter + "%") ||
                                EF.Functions.Like(acc.email, "%" + filter + "%") ||
                                EF.Functions.Like(acc.descPelapor, "%" + filter + "%") ||
                                EF.Functions.Like(acc.nomorTelp, "%" + filter + "%") ||
                                EF.Functions.Like(acc.namaProduk, "%" + filter + "%"))
                        )
                    .OrderByDescending(p => p.CreatedAt).ToPagedList(pg, pageSize);
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
        }
        #endregion

        #region Navbar menu, profile, setting, etc..
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
        #endregion

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        #region Export
        //Print
        [Obsolete]
        [Authorize]
        [Route("~/Admin/Pengaduan/Export")]
        public FileResult exportPengaduan(string fromDate, string toDate, string filter)
        {
            List<ReportProductDTO> dataExport = new List<ReportProductDTO>();
            MemoryStream result = new MemoryStream();
            
            try
            {
                fromDate = fromDate == null ? "" : fromDate.Trim();
                toDate = toDate == null ? "" : toDate.Trim();
                filter = filter == null ? "" : filter.Trim();

                DateTime dtFrom = DateTime.Now;
                DateTime dtTo = DateTime.Now;

                try
                {
                    dtFrom = DateTime.Now;
                    dtFrom = fromDate.Trim() == "" ? DateTime.Now : DateTime.Parse(fromDate.Trim());
                    dtTo = toDate == null ? DateTime.Now : DateTime.Parse(toDate.Trim()).AddDays(1);
                    fromDate = fromDate.Trim() == "" ? "" : DateTime.Parse(fromDate.Trim()).ToString("yyyy-MM-dd");
                    toDate = toDate.Trim() == "" ? "" : DateTime.Parse(toDate.Trim()).ToString("yyyy-MM-dd");
                }
                catch
                {
                    fromDate = "";
                    toDate = "";
                }

                //Get Data
                if ((filter == null ? "" : filter.Trim()) == "")
                {
                    if (fromDate.Trim() != "" && toDate.Trim() != "")
                    {
                        dataExport = _context.report_product.AsNoTracking().Where(acc => acc.CreatedAt >= dtFrom && acc.CreatedAt < dtTo).OrderByDescending(p => p.CreatedAt).ToList();
                    }
                    else
                    {
                        dataExport = _context.report_product.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToList();
                    }
                }
                else
                {
                    if (fromDate.Trim() != "" && toDate.Trim() != "")
                    {
                        dataExport = _context.report_product.AsNoTracking()
                        .Where(
                            acc =>
                                acc.CreatedAt >= dtFrom && acc.CreatedAt < dtTo &&
                                (EF.Functions.Like(acc.namaLengkap, "%" + filter + "%") ||
                                EF.Functions.Like(acc.email, "%" + filter + "%") ||
                                EF.Functions.Like(acc.descPelapor, "%" + filter + "%") ||
                                EF.Functions.Like(acc.nomorTelp, "%" + filter + "%") ||
                                EF.Functions.Like(acc.namaProduk, "%" + filter + "%"))
                            )
                        .OrderByDescending(p => p.CreatedAt).ToList();
                    }
                    else
                    {
                        dataExport = _context.report_product.AsNoTracking()
                        .Where(
                            acc =>
                                EF.Functions.Like(acc.namaLengkap, "%" + filter + "%") ||
                                EF.Functions.Like(acc.email, "%" + filter + "%") ||
                                EF.Functions.Like(acc.descPelapor, "%" + filter + "%") ||
                                EF.Functions.Like(acc.nomorTelp, "%" + filter + "%") ||
                                EF.Functions.Like(acc.namaProduk, "%" + filter + "%")
                            )
                        .OrderByDescending(p => p.CreatedAt).ToList();
                    }
                }

                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                _export.exportPengaduan(fromDate, toDate, filter, dataExport, ref result);
            }
            catch (Exception exc)
            {

            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Pengaduan.xlsx");
        }

        [Obsolete]
        [Authorize]
        [Route("~/Admin/Products/Export")]
        public FileResult exportProducts(string filter)
        {
            List<SeriesMasterDTO> dataExport = new List<SeriesMasterDTO>();
            MemoryStream result = new MemoryStream();

            try
            {
                filter = filter == null ? "" : filter.Trim();

                //Get Data
                if ((filter == null ? "" : filter.Trim()) == "")
                {
                    dataExport =  _context.series_master.AsNoTracking().OrderBy(p => p.seriesID).ToList();
                }
                else
                {
                    dataExport =  _context.series_master.AsNoTracking()
                        .Where(
                            acc =>
                            EF.Functions.Like(acc.seriesID, "%" + filter + "%") ||
                            EF.Functions.Like(acc.productName, "%" + filter + "%") ||
                            EF.Functions.Like(acc.productPackaging, "%" + filter + "%") ||
                            EF.Functions.Like(acc.productVolume, "%" + filter + "%")
                    )
                        .OrderBy(p => p.seriesID).ToList();
                }

                _export.exportProduct(filter, dataExport, ref result);
            }
            catch (Exception exc)
            {

            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
        }

        [Obsolete]
        [Authorize]
        [Route("~/Admin/Scan/Export")]
        public FileResult exportScanAsync(string filter)
        {
            List<LogScanningDTO> dataExport = new List<LogScanningDTO>();
            MemoryStream result = new MemoryStream();

            try
            {
                filter = filter == null ? "" : filter.Trim();

                //Get Data
                if ((filter == null ? "" : filter.Trim()) == "")
                {
                    dataExport =  _context.log_scanning.AsNoTracking().OrderByDescending(p => p.CreatedAt).ToList();
                }
                else
                {
                    dataExport =  _context.log_scanning.AsNoTracking()
                        .Where(
                            acc =>
                                EF.Functions.Like(acc.productId, "%" + filter + "%") ||
                                EF.Functions.Like(acc.qrNo, "%" + filter + "%")
                    )
                        .OrderByDescending(p => p.CreatedAt).ToList();
                }

                _export.exportScan(filter, dataExport, ref result);
            }
            catch (Exception exc)
            {

            }

            return File(result, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Scans.xlsx");
        }
        #endregion




    }
}
