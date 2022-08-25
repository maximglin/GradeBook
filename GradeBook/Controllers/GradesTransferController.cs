using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GradeBook.Controllers
{

    [Authorize(Policy = "Admin")]
    public class GradesTransferController : Controller
    {

        IGradeBookContext _context;
        public GradesTransferController(IGradeBookContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (User.Identity is null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

            if (user is null) return RedirectToAction("Index", "Home");



            var model = new Dictionary<int, (List<
(GradeBook.Storage.Entities.Group Group, string Teacher)
> Groups, Subject Subject)>();
            var model2 = new Dictionary<int, (List<
(GradeBook.Storage.Entities.Group Group, string Teacher)
> Groups, Subject Subject)>();
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

            var grades = await _context.Grades.ToListAsync();

            foreach (var g in rels.GroupBy(r => r.SubjectId))
            {
                if (g.Count() == 0) continue;

                model.Add(g.Key, (new List<(Group Group, string Teacher)>(), g.First().Subject));
                model2.Add(g.Key, (new List<(Group Group, string Teacher)>(), g.First().Subject));
                foreach (var val in g.GroupBy(x => x.GroupId))
                {
                    string teachers = "";
                    foreach (var v2 in val)
                    {
                        teachers += (teachers == "" ? "" : ", ") + v2.Teacher.Name;
                    }

                    if(grades.Any(g => val.First().Group.Students.Contains(g.Student) &&
                        g.Subject == val.First().Subject &&
                        (g.State == GradeState.Unlocked || g.State == GradeState.NeedsApproval)
                    ))
                        model[g.Key].Groups.Add((val.First().Group, teachers));
                    else
                        model2[g.Key].Groups.Add((val.First().Group, teachers));
                }
            }




            return View(new GradesTransferViewModel()
            {
                Item1 = model,
                Item2 = model2
            });
        }


        [HttpGet]
        public async Task<IActionResult> TransferGrades(int subjectId, int groupId)
        {
            if (User.Identity is null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

            if (user is null) return RedirectToAction("Index", "Home");


            //if (!user.IsAdmin)
            //{
            //    var rels = await _context.TeacherSubjectGroupRelations
            //    .OrderBy(r => r.Subject.Name)
            //    .ThenBy(r => r.Group.Name)
            //    .ToListAsync();

            //    var old = rels;
            //    rels = rels.Where(r => user.Teachers.Contains(r.Teacher)).ToList();

            //    if (!rels.Any(r => r.SubjectId == subjectId && r.GroupId == groupId))
            //        return RedirectToAction("Index");
            //}


            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if (subject is null || group is null) return RedirectToAction("Index");


            var model = new GradesViewModel();

            model.Subject = subject;
            model.Group = group;
            model.Grades = group.Students.OrderBy(s => s.Name).Select(s =>
            new GradeViewModel()
            {
                Student = s,
                M1 = _context.Grades.Where(
                    g => g.SubjectId == subject.Id &&
                    g.StudentId == s.Id &&
                    g.Type == GradeType.M1
                    ).FirstOrDefault()?.Value ?? null,
                M2 = _context.Grades.Where(
                    g => g.SubjectId == subject.Id &&
                    g.StudentId == s.Id &&
                    g.Type == GradeType.M2
                    ).FirstOrDefault()?.Value ?? null,

                M1State = _context.Grades.Where(
                    g => g.SubjectId == subject.Id &&
                    g.StudentId == s.Id &&
                    g.Type == GradeType.M1
                    ).FirstOrDefault()?.State ?? GradeState.Unlocked,

                M2State = _context.Grades.Where(
                    g => g.SubjectId == subject.Id &&
                    g.StudentId == s.Id &&
                    g.Type == GradeType.M2
                    ).FirstOrDefault()?.State ?? GradeState.Unlocked
            }).ToList();

            model.IsAdminEditing = user.IsAdmin;

            return View("TransferGrades", model);
        }

        [HttpPost]
        public async Task<IActionResult> TransferGrades(GradesViewModel model)
        {
            foreach(var grade in model.Grades)
            {
                var grade_to_change_stateM1 = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.StudentId == grade.Student.Id &&
                    g.SubjectId == model.Subject.Id &&
                    g.Type == GradeType.M1
                    );

                var grade_to_change_stateM2 = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.StudentId == grade.Student.Id &&
                    g.SubjectId == model.Subject.Id &&
                    g.Type == GradeType.M2
                    );


                if (grade_to_change_stateM1 is not null && grade.M1State is not null)
                { 
                    grade_to_change_stateM1.State = grade.M1State.Value;
                    if (grade.M1 is not null)
                        grade_to_change_stateM1.Value = grade.M1.Value;
                }

                if (grade_to_change_stateM2 is not null && grade.M2State is not null)
                { 
                    grade_to_change_stateM2.State = grade.M2State.Value;
                    if (grade.M2 is not null)
                        grade_to_change_stateM2.Value = grade.M2.Value;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
