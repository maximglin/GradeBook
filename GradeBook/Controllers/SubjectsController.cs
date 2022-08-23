using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Controllers
{
    [Authorize(Policy = "Admin")]
    public class SubjectsController : Controller
    {
        IGradeBookContext _context;

        public SubjectsController(IGradeBookContext context)
        {
            _context = context;
        }


        private async Task<IEnumerable<Subject>> GetModelAsync()
        {
            return await _context.Subjects.OrderBy(s => s.Name).ToListAsync();
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetModelAsync());
        }

        public async Task<IActionResult> Remove(int id)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);
            if (subject is not null)
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
            return View("Index", await GetModelAsync());
        }

        public async Task<IActionResult> Add(Subject t)
        {
            {
                _context.Subjects.Add(t);
                await _context.SaveChangesAsync();
            }

            ModelState.Clear();
            return View("Index", await GetModelAsync());
        }

        [HttpGet]
        public async Task<IActionResult> EditGroups(int id)
        {

            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == id);

            if (subject is not null)
            {
                var model = new EditGroupsViewModel();
                model.Subject = subject;
                model.Groups = await _context.Groups.Select(g =>
                new SelectedGroupViewModel()
                {
                    Group = g,
                    IsSelected = subject.Groups.Contains(g)
                }).OrderBy(g => g.Group.Name).ThenByDescending(g => g.IsSelected).ToListAsync();

                

                return View("EditGroups", model);
            }

            ModelState.Clear();
            return View("Index", await GetModelAsync());
            
        }

        [HttpPost]
        public async Task<IActionResult> EditGroups(EditGroupsViewModel model)
        {
            var subject = await _context.Subjects.FirstOrDefaultAsync(x => x.Id == model.Subject.Id);

            if(subject is not null)
            {
                var groups = await _context.Groups.ToListAsync();
                groups = groups.Where(g => model.Groups.Any(sg => sg.IsSelected && sg.Group.Id == g.Id)).ToList();

                subject.Groups.Clear();
                subject.Groups.AddRange(groups);

                var to_fix_remove = _context.TeacherSubjectGroupRelations.Where(r => r.Subject == subject && !subject.Groups.Contains(r.Group));
                _context.TeacherSubjectGroupRelations.RemoveRange(to_fix_remove);

                await _context.SaveChangesAsync();
            }
            

            return View("Index", await GetModelAsync());
        }

    }
}
