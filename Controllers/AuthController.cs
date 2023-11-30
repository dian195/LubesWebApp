using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI.Common;
using System.Security.Claims;
using WebApp.Models;
using WebApp.Repository;
using System;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDB _context;

        public AuthController(AppDB context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("~/Auth/SignIn")]
        public IActionResult SignIn()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            userId = userId == null ? "" : userId;
            if (userId.Trim() != "")
            {
                return Redirect("~/Admin/Index");
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> SignIn(SignInDTO user)
        {
            if (ModelState.IsValid)
            {
                UserDTO usr = new UserDTO();

                string encPassword = EncryptionHelper.Encrypt(user.password);

                if (user.usernameOrEmail.Contains("@"))
                {
                    usr = await _context.account.Include(e => e.Role).Where(s => s.email == user.usernameOrEmail && s.password == encPassword && s.isActive == 1 && s.Role.IsActive == 1).FirstOrDefaultAsync();
                }
                else
                {
                    usr = await _context.account.Include(e => e.Role).Where(s => s.username == user.usernameOrEmail && s.password == encPassword && s.isActive == 1 && s.Role.IsActive == 1).FirstOrDefaultAsync();
                }

                if (usr == null)
                {
                    ViewBag.msg = "Login or password is incorrect";
                    return View(user);
                }

                //update last login
                var dtusr = _context.account.Find(usr.userId);
                if (dtusr != null)
                {
                    dtusr.lastLogin = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                var claims = new[] { 
                    new Claim(ClaimTypes.Sid, usr.userId.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, usr.username),
                    new Claim(ClaimTypes.Name, usr.namaUser),
                    new Claim(ClaimTypes.Role, usr.Role.Role_Name.ToString()),
                    new Claim(ClaimTypes.Email, usr.email)};

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(
                                          CookieAuthenticationDefaults.AuthenticationScheme,
                                          new ClaimsPrincipal(identity),
                                          new AuthenticationProperties
                                          {
                                              IsPersistent = user.RememberMe   //remember me
                                          });

                return Redirect("~/Admin/Index");
            }

            return View(user);

        }

        [Route("~/Auth/SignOut")]
        public async Task<IActionResult> SignOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("~/Auth/SignIn");

            //await _signInManager.SignOutAsync();
            //return RedirectToAction(nameof(SignIn));
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(UserDTO register)
        {
            //if (!ModelState.IsValid) return View();

            //IdentityUser newUser = new IdentityUser
            //{
            //    Email = register.email,
            //    UserName = register.username
            //};

            //IdentityResult result = await _userManager.CreateAsync(newUser, register.password);
            //if (!result.Succeeded)
            //{
            //    foreach (var item in result.Errors)
            //    {
            //        ModelState.AddModelError("", item.Description);
            //    }
            //}
            return RedirectToAction("SignIn");
        }

    }
}
