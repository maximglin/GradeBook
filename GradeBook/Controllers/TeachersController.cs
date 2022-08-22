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

        public async Task<IActionResult> Index()
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

            return View(model);
        }
    }
}
