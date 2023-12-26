using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Org.BouncyCastle.Ocsp;
using System;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Repository;

namespace WebApp.Controllers
{
    public class APIController : ControllerBase
    {
        private readonly AppDB _context;
        private readonly string _captchaSecretKey;

        public APIController(AppDB context, IOptions<AppSettings> appSettings)
        {
            _captchaSecretKey = appSettings.Value.CaptchaSecretKey;
            _context = context;
        }

        [HttpPost]
        public IActionResult PostLogScanning([FromBody] LogScanningDTO logScanning)
        {
            if (logScanning == null)
            {
                return Ok(new { Id = 0, Message = "Data kosong", logScanning });
            }

            try
            {
                logScanning.CreatedAt = DateTime.Now;

                _context.log_scanning.Add(logScanning);
                _context.SaveChanges();

                return Ok(new { Id = 1, Message = "Data berhasil disimpan", logScanning });
            }
            catch (Exception ex)
            {
                return Ok(new { Id = 0, Message = ex.Message, logScanning });
            }
        }

        [HttpPost]
        public IActionResult PostReportProduct([FromBody] ReportProductDTO dto)
        {
            if (dto == null)
            {
                return Ok(new { Id = 0, Message = "Data kosong", dto });
            }

            // Verify reCAPTCHA response
            var captchaSecretKey = _captchaSecretKey;
            var client = new HttpClient();
            var response = client.GetStringAsync($"https://www.google.com/recaptcha/api/siteverify?secret={captchaSecretKey}&response={dto.gRecaptchaResponse}").Result;

            // Parse and handle the response
            var result = JsonConvert.DeserializeObject<RecaptchaResponse>(response);

            if (result.Success)
            {
                try
                {
                    dto.CreatedAt = DateTime.Now;

                    _context.report_product.Add(dto);
                    _context.SaveChanges();

                    return Ok(new { Id = 1, Message = "Laporan berhasil disubmit", dto });
                }
                catch (Exception ex)
                {
                    return Ok(new { Id = 0, Message = ex.Message, dto });
                }
            }
            else
            {
                // reCAPTCHA verification failed, show an error
                return BadRequest(new { id = 0, message = "reCAPTCHA verification failed." });
            }
        }

        [HttpPost]
        public IActionResult UpdateProfile([FromBody] UserProfileDTO dto)
        {
            if (dto == null)
            {
                return Ok(new { Id = 0, Message = "Data kosong", dto });
            }
            try
            {
                var cekEmail = _context.account.Where(e => e.email == dto.email && e.userId != dto.userId).ToList();
                if (cekEmail.Count > 0)
                {
                    return Ok(new { Id = 0, Message = "Email sudah digunakan!", dto });
                }

                var dtusr = _context.account.Find(dto.userId);
                if (dtusr != null)
                {
                    dtusr.namaUser = dto.namaUser;
                    dtusr.roleId = dto.roleId;
                    dtusr.email = dto.email;
                    dtusr.username = dto.username;
                    dtusr.lastUpdate = DateTime.Now;
                    dtusr.lastUpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.SaveChangesAsync();
                }
                else
                {
                    return Ok(new { Id = 0, Message = "Data tidak ditemukan", dto });
                }

                return Ok(new { Id = 1, Message = "Profil berhasil diupdate", dto });
            }
            catch (Exception ex)
            {
                return Ok(new { Message = ex.Message, dto });
            }
        }

        [HttpPost]
        public IActionResult UpdatePasswordProfile([FromBody] UpdatePassProfileDTO dto)
        {
            if (dto == null)
            {
                return Ok(new { Id = 0, Message = "Data tidak ditemukan", dto });
            }

            if (dto.newpass.Trim() != dto.confirmpass.Trim())
            {
                return Ok(new { Id = 0, Message = "Konfirmasi Password tidak sama !", dto });
            }

            try
            {
                string encOldPassword = EncryptionHelper.Encrypt(dto.oldpass);
                string encNewPassword = EncryptionHelper.Encrypt(dto.newpass);

                var dtusr2 = _context.account.Where(e => e.userId == dto.usrId && e.password == encOldPassword).FirstOrDefault();
                if (dtusr2 == null)
                {
                    return Ok(new { Id = 0, Message = "Password tidak sesuai", dto });
                }

                var dtusr = _context.account.Find(dto.usrId);
                if (dtusr != null)
                {
                    dtusr.password = encNewPassword;
                    dtusr.lastUpdate = DateTime.Now;
                    dtusr.lastUpdateBy = User.FindFirstValue(ClaimTypes.NameIdentifier);
                    _context.SaveChangesAsync();
                }
                else
                {
                    return Ok(new { Id = 0, Message = "Password tidak sesuai", dto });
                }

                return Ok(new { Id = 1, Message = "Password berhasil diupdate", dto });
            }
            catch (Exception ex)
            {
                return Ok(new { Id = 0, Message = ex.Message, dto });
            }
        }

        [HttpGet]
        [Consumes("application/json")]
        public IActionResult dashboardItem()
        {
            DashboardDTO dto = new DashboardDTO();
            dto.reportProduct = new List<ReportProductDTO>();
            dto.logScanning = new List<LogScanningDTO>();

            try
            {
                //get total scan
                dto.totalScan = _context.log_scanning.Count();
                dto.totalPengaduan = _context.report_product.Count();
                dto.totalProduct = _context.series_master.Count();
                dto.logScanning = _context.log_scanning.OrderByDescending(e => e.CreatedAt).Take(5).ToList();
                dto.reportProduct = _context.report_product.OrderByDescending(e => e.CreatedAt).Take(5).ToList();

                return Ok(new { Id = 1, Message = "", dto });
            }
            catch (Exception ex)
            {
                return Ok(new { Id = 0, Message = ex.Message, dto });
            }
        }

    }
}
