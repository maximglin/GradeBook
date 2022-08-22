using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GradeBook.Controllers
{
    [Authorize(Policy = "Admin")]
    public class UsersController : Controller
    {
        IGradeBookContext _context;

        public UsersController(IGradeBookContext context)
        {
            _context = context;
        }


        public IActionResult Index()
        {


            return View();
        }
    }
}
