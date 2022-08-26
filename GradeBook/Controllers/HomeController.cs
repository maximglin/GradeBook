using GradeBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Controllers
{
    public class HomeController : Controller
    {
        IGradeBookContext _context;

        public HomeController(IGradeBookContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);
                if(user is not null)
                {
                    if (user.IsAdminOrGradeTransfer)
                        return RedirectToAction("Index", "GradesTransfer");
                    else
                        return RedirectToAction("Index", "Grades");
                }
                else
                {
                    await HttpContext.SignOutAsync();
                }
            }
                

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> ChangePassword(string login = "", bool firstLogin = false)
        {
            if (User.Identity is not null && User.Identity.IsAuthenticated)
            { 
                await HttpContext.SignOutAsync();
                return RedirectToAction("ChangePassword", new { login = login, firstLogin = firstLogin });
            }

            return View("ChangePassword", new ChangePasswordViewModel()
            {
                Login = login,
                FirstChange = firstLogin
            });
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.Login == model.Login &&
                u.Password == model.OldPassword);
            if (user is not null)
            {
                if(model.NewPassword != user.Password &&
                    model.NewPassword.Length >= 5 &&
                    model.NewPassword == model.NewPasswordRepeat)
                {
                    user.FirstLogin = false;
                    user.Password = model.NewPassword;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> LogIn(UserCredentials user)
        {
            var us = await _context.Users.FirstOrDefaultAsync(
                u => u.Login == user.Login &&
                u.Password == user.Password);
            if (us is not null)
            {
                //if (us.FirstLogin)
                //    return RedirectToAction("ChangePassword", new {login = us.Login, firstLogin = true});

                await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new System.Security.Claims.ClaimsPrincipal(new UserIdentity()
                {
                    Name = us.Login,
                    IsAuthenticated = true
                }
                ));
            }

            
            var referer = Request.Headers["referer"];
            if (referer.Any(r => r.Contains("ChangePassword")))
                return RedirectToAction("Index");

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