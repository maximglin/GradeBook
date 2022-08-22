using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }
    }
}
