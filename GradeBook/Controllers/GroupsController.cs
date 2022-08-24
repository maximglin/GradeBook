using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Controllers
{
    [Authorize(Policy = "Admin")]
    public class GroupsController : Controller
    {
        IGradeBookContext _context;

        public GroupsController(IGradeBookContext context)
        {
            _context = context;
        }

        private async Task<List<Group>> GetModelAsync()
        {
            return await _context.Groups.OrderBy(g => g.Name).ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            return View("Index", await GetModelAsync());
        }

        public async Task<IActionResult> Add(Group group)
        {
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            ModelState.Clear();
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if(group is not null)
            {
                _context.Groups.Remove(group);
                await _context.SaveChangesAsync();
            }
            

            ModelState.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditStudents(int id)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == id);
            if(group is not null)
            {
                var model = new EditStudentsViewModel();
                model.Group = group;
                model.Students = string.Join("\r\n", group.Students.Select(s => s.Name).OrderBy(n => n));

                return View("EditStudents", model);
            }

            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> EditStudents(EditStudentsViewModel model)
        {
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == model.Group.Id);
            if (group is not null)
            {
                var studentNames = model.Students?.Split("\r\n") ?? Array.Empty<string>();
                group.Students.Clear();
                group.Students = studentNames.Where(n => n.Length > 0).Select(n => new Student()
                {
                    Group = group,
                    Name = n
                }).ToList();
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}
