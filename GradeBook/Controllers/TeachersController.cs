using GradeBook.Models;
using GradeBook.Storage;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Controllers
{
    [Authorize(Policy = "Admin")]
    public class TeachersController : Controller
    {
        IGradeBookContext _context;

        public TeachersController(IGradeBookContext context)
        {
            _context = context;
        }

        async Task<List<TeacherViewModel>> GetModelAsync()
        {
            var model = new List<TeacherViewModel>();

            foreach (var t in (await _context.Teachers.ToListAsync()))
            {
                model.Add(new TeacherViewModel()
                {
                    Teacher = t,
                    SubjectGroups = await _context.TeacherSubjectGroupRelations
                    .Where(q => q.Teacher == t).ToListAsync()
                });
            }

            return model;
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetModelAsync());
        }

        public async Task<IActionResult> Remove(int id)
        {
            var t = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == id);
            if (t is not null)
            {
                _context.Teachers.Remove(t);
                await _context.SaveChangesAsync();
            }

            return View("Index", await GetModelAsync());
        }
    }
}
