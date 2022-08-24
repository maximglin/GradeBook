using GradeBook.Models;
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

        [HttpGet]
        public async Task<IActionResult> SetGrades(int subjectId, int groupId)
        {
            if (User.Identity is null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

            if (user is null) return RedirectToAction("Index", "Home");


            if (!user.IsAdmin)
            {
                var rels = await _context.TeacherSubjectGroupRelations
                .OrderBy(r => r.Subject.Name)
                .ThenBy(r => r.Group.Name)
                .ToListAsync();

                var old = rels;
                rels = rels.Where(r => user.Teachers.Contains(r.Teacher)).ToList();

                if(!rels.Any(r => r.SubjectId == subjectId && r.GroupId == groupId))
                    return RedirectToAction("Index");
            }


            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == subjectId);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == groupId);

            if(subject is null || group is null) return RedirectToAction("Index");


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



            return View("SetGrades", model);
        }

        [HttpPost]
        public async Task<IActionResult> SetGrades(GradesViewModel model)
        {
            if (User.Identity is null) return RedirectToAction("Index", "Home");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == User.Identity.Name);

            if (user is null) return RedirectToAction("Index", "Home");

            var subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Id == model.Subject.Id);
            var group = await _context.Groups.FirstOrDefaultAsync(g => g.Id == model.Group.Id);
            if(subject is null || group is null) return RedirectToAction("Index");

            if (!user.IsAdmin)
            {
                var rels = await _context.TeacherSubjectGroupRelations
                .OrderBy(r => r.Subject.Name)
                .ThenBy(r => r.Group.Name)
                .ToListAsync();

                var old = rels;
                rels = rels.Where(r => user.Teachers.Contains(r.Teacher)).ToList();

                if (!rels.Any(r => r.SubjectId == subject.Id && r.GroupId == group.Id))
                    return RedirectToAction("Index");
            }

            var grades = new List<Grade>();
            var grades_to_remove = new List<Grade>();
            foreach(var grade in model.Grades)
            {

                var student = await _context.Students.FirstOrDefaultAsync(s => s.Id == grade.Student.Id);
                
                if (student is null || subject is null) continue;

                var grade_for_checkM1 = await _context.Grades.FirstOrDefaultAsync(g => g.StudentId == student.Id && g.SubjectId == model.Subject.Id && g.Type == GradeType.M1);
                var grade_for_checkM2 = await _context.Grades.FirstOrDefaultAsync(g => g.StudentId == student.Id && g.SubjectId == model.Subject.Id && g.Type == GradeType.M2);

                if (grade_for_checkM1 is null || grade_for_checkM1.State == GradeState.Unlocked)
                {
                    if (grade.M1 is not null)
                    {
                        grades.Add(new Grade()
                        {
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            Student = student,
                            Subject = subject,
                            Type = GradeType.M1,
                            Value = grade.M1.Value,
                            State = GradeState.Unlocked
                        });
                    }
                    else
                    {
                        grades_to_remove.Add(new Grade()
                        {
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            Student = student,
                            Subject = subject,
                            Type = GradeType.M1,
                            State = GradeState.Unlocked
                        });
                    }
                }
                if (grade_for_checkM2 is null || grade_for_checkM2.State == GradeState.Unlocked)
                {
                    if (grade.M2 is not null)
                    {
                        grades.Add(new Grade()
                        {
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            Student = student,
                            Subject = subject,
                            Type = GradeType.M2,
                            Value = grade.M2.Value,
                            State = GradeState.Unlocked
                        });
                    }
                    else
                    {
                        grades_to_remove.Add(new Grade()
                        {
                            StudentId = student.Id,
                            SubjectId = subject.Id,
                            Student = student,
                            Subject = subject,
                            Type = GradeType.M2,
                            State = GradeState.Unlocked
                        });
                    }
                }
            }

            foreach(var grade in grades_to_remove)
            {
                var to_remove = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.SubjectId == grade.SubjectId &&
                    g.StudentId == grade.StudentId &&
                    g.Type == grade.Type
                    );
                if (to_remove is not null)
                    _context.Grades.Remove(to_remove);
            }

            foreach(var grade in grades)
            {
                var to_edit = await _context.Grades.FirstOrDefaultAsync(g =>
                    g.SubjectId == grade.SubjectId &&
                    g.StudentId == grade.StudentId &&
                    g.Type      == grade.Type
                    );
                if (to_edit is not null)
                    to_edit.Value = grade.Value;
                else
                    _context.Grades.Add(grade);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
