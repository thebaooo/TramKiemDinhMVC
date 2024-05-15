using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Test.Models;
using Test.Data;

namespace Test.Controllers
{

    public class AccessController : Controller
    {
        private readonly TestContext _context;

        public AccessController(TestContext context)
        {
            _context = context;
        }
        public IActionResult Login()
        {
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(User _userFromPage)
        {
            var _user = _context.User
                        .Include(u => u.Role) // Đảm bảo rằng dữ liệu Role được load
                        .FirstOrDefault(m => m.UserName == _userFromPage.UserName && m.Password == _userFromPage.Password);
            if (_user == null)
            {
                ViewBag.LoginStatus = 0;
            }
            else
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, _user.UserName),
                new Claim("FullName", _user.UserName),
                //new Claim("OtherProperties","Example Role"),
                new Claim(ClaimTypes.Role, _user.Role.RoleName),
                new Claim("ProvinceName", _user.ProvinceName),
            };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = _userFromPage.KeepLoggedIn,
                };

                await HttpContext.SignInAsync(
                   CookieAuthenticationDefaults.AuthenticationScheme,
                   new ClaimsPrincipal(claimsIdentity),
                   authProperties);
                return RedirectToAction("Index", "Home");
            }
            ViewData["ValidateMessage"] = "Thông tin đăng nhập sai";
            return View();
        }
        
    }
}
