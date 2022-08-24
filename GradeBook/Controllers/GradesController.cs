using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GradeBook.Controllers
{

    [Authorize]
    public class GradesController : Controller
    {
        IGradeBookContext _context;
        public GradesController(IGradeBookContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity is null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

            if(user is null) return RedirectToAction("Index", "Home");



            var model = new Dictionary<int, (List<
(GradeBook.Storage.Entities.Group Group, string Teacher)
> Groups, string Name)>();
            var rels = await _context.TeacherSubjectGroupRelations
                .OrderBy(r => r.Subject.Name)
                .ThenBy(r => r.Group.Name)
                .ToListAsync();

            if (!user.IsAdmin)
            {
                var old = rels;
                rels = rels.Where(r => user.Teachers.Contains(r.Teacher)).ToList();
                rels = rels.Concat(old.Where(r => rels.Any(x => x.SubjectId == r.SubjectId
                && x.GroupId == r.GroupId))).Distinct().ToList();
            }

            

            foreach(var g in rels.GroupBy(r => r.SubjectId))
            {
                if (g.Count() == 0) continue;

                model.Add(g.Key, (new List<(Group Group, string Teacher)>(), g.First().Subject.Name));
                foreach (var val in g.GroupBy(x => x.GroupId))
                {
                    string teachers = "";
                    foreach(var v2 in val)
                    {
                        teachers += (teachers == "" ? "" : ", ") + v2.Teacher.Name;
                    }
                    model[g.Key].Groups.Add((val.First().Group, teachers));
                }
            }
            


            return View(model);
        }
    }
}
