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


        public async Task<IActionResult> Remove(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if(user is not null && user.Login != User.Identity.Name && user.Login != "admin")
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return View("Index", await _context.Users.ToListAsync());
        }
    }
}
