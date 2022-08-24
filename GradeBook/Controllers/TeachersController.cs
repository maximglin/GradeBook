using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
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

            foreach (var t in (await _context.Teachers.OrderBy(tc => tc.Name).ToListAsync()))
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

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Add(Teacher t)
        {
            {
                _context.Teachers.Add(t);
                await _context.SaveChangesAsync();
            }

            ModelState.Clear();
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditSubjects(int id)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.Id == id);
            if(teacher is not null)
            {
                var model = new EditTeacherSubjectsViewModel();

                model.Teacher = teacher;
                foreach(var s in await _context.Subjects.OrderBy(s => s.Name).ToListAsync())
                {
                    var list = await _context.Groups.Where(g => s.Groups.Contains(g))
                        .Select(g => new SelectedSubjectGroupViewModel()
                        {
                            Subject = s,
                            Group = g
                        }).OrderBy(g => g.Group.Name).ToListAsync();

                    foreach (var item in list)
                        item.IsSelected = await _context.TeacherSubjectGroupRelations.AnyAsync(
                            r => r.Teacher == teacher &&
                            r.Subject == item.Subject &&
                            r.Group == item.Group
                            );

                    model.Selected.Add(s.Id, list);
                }


                return View("EditSubjects", model);
            }


            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> EditSubjects(EditTeacherSubjectsViewModel model)
        {
            var teacher = await _context.Teachers.FirstOrDefaultAsync(x => x.Id == model.Teacher.Id);

            if(teacher is not null)
            {
                IEnumerable<SelectedSubjectGroupViewModel> rels = new List<SelectedSubjectGroupViewModel>();
                foreach (var r in model.Selected.Values)
                    rels = rels.Concat(r);



                _context.TeacherSubjectGroupRelations.RemoveRange(
                        _context.TeacherSubjectGroupRelations.Where(rel =>
                            rel.Teacher == teacher
                            )
                        );
                foreach (var r in rels.Where(r => r.IsSelected))
                {
                    var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == r.Subject.Id);
                    var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == r.Group.Id);


                    if (subject is not null &&
                        group is not null)
                        _context.TeacherSubjectGroupRelations.Add(new TeacherSubjectGroup()
                        {
                            Teacher = teacher,
                            Subject = subject,
                            Group = group
                        });
                }

                await _context.SaveChangesAsync();
            }


            return RedirectToAction("Index");
        }
    }
}
