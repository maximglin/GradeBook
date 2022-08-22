using GradeBook.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;

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