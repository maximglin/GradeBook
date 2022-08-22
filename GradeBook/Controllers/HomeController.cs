using GradeBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace GradeBook.Controllers
{
    public class HomeController : Controller
    {
        IGradeBookContext _context;

        public HomeController(IGradeBookContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> LogIn(UserCredentials user)
        {
            if(_context.Users.FirstOrDefault(
                u => u.Login == user.Login && 
                u.Password == user.Password) 
                is not null)
            {
                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(new UserIdentity()
                {
                    Name = user.Login,
                    IsAuthenticated = true
                }
                ));
            }

            
            var referer = Request.Headers["referer"];
            return Redirect(referer);
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
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