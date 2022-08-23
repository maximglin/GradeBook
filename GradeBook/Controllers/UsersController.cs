using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
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
            return View(await GetModel());
        }

        private async Task<List<User>> GetModel()
        {
            return await _context.Users.OrderByDescending(u => u.IsAdmin).ThenBy(u => u.Login).ToListAsync();
        }

        public async Task<IActionResult> Remove(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if(user is not null && user.Login != User.Identity.Name && user.Login != "admin")
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            ModelState.Clear();
            return View("Index", await GetModel());
        }
        public async Task<IActionResult> Add(User user)
        {
            if(user is not null && user.Password is not null && 
                user.Login is not null && user.Login.Length > 1 &&
                user.Password.Length > 1 && !_context.Users.Any(u => u.Login == user.Login))
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            ModelState.Clear();
            return View("Index", await GetModel());
        }

        [HttpGet]
        public async Task<IActionResult> EditTeachers(int id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if(user is not null && !user.IsAdmin)
            {
                var model = (await
                    _context.Teachers.OrderBy(t => t.Name).Select(t =>
                    new SelectedTeacherViewModel { Teacher = t, Selected = user.Teachers.Contains(t) }
                    ).OrderByDescending(t => t.Selected).ToListAsync());

                return View("EditTeachers", new EditUserTeachersViewModel()
                {
                    UserId = user.Id,
                    Teachers = model
                });
            }
            else
                return View("Index", await GetModel());
        }

        [HttpPost]
        public async Task<IActionResult> EditTeachers(EditUserTeachersViewModel model)
        {
            var id = model.UserId;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user is not null && !user.IsAdmin)
            {
                var teachers = model.Teachers.Where(item => item.Selected).Select(item => item.Teacher.Id).ToList();

                user.Teachers.Clear();
                user.Teachers.AddRange(_context.Teachers.Where(t => teachers.Contains(t.Id)));
                await _context.SaveChangesAsync();
            }
            
            return View("Index", await GetModel());
        }

    }
}
