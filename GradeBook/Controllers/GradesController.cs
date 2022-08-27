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


            var rels = await _context.TeacherSubjectGroupRelations.ToListAsync();

            var subjects = await _context.Subjects.OrderBy(s => s.Name).ToListAsync();

            Dictionary<int, (List<(GradeBook.Storage.Entities.Group Group, string Teacher)> Groups, string Name)> dic;
                
            
            if(user.IsAdmin)
            {
                dic = subjects.ToDictionary(s => s.Id, s => (s.Groups.OrderBy(g => g.Name).Select(g => (g,
                string.Join(", ", rels.Where(r => r.SubjectId == s.Id && r.GroupId == g.Id).Select(r => r.Teacher.Name))
                )).ToList(), s.Name));
            }
            else
            {
                dic = subjects.Where(s => rels.Any(r => r.SubjectId == s.Id && user.Teachers.Contains(r.Teacher) && s.Groups.Contains(r.Group)))
                    .ToDictionary(s => s.Id, s => (s.Groups
                    .Where(g => rels.Any(r => r.SubjectId == s.Id && user.Teachers.Contains(r.Teacher) && r.GroupId == g.Id))
                    .OrderBy(g => g.Name).Select(g => (g,
                string.Join(", ", rels.Where(r => r.SubjectId == s.Id && r.GroupId == g.Id).Select(r => r.Teacher.Name))
                )).ToList(), s.Name));
            }
            


            return View(dic);
        }


        private async Task<bool> IsPageBlocked(int subjectId, int groupId)
        {
            var block = await _context.BlockedPages.FirstOrDefaultAsync(b => b.SubjectId == subjectId && b.GroupId == groupId);
            
            if (block is null) return false;

            if (DateTime.UtcNow - block.TimeStamp > TimeSpan.FromMinutes(2)) return false;

            return true;
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

            if(await IsPageBlocked(subjectId, groupId))
            {
                if(!user.IsAdmin)
                    return RedirectToAction("Index"); //error page is watched by transfer
            }
                


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

            if (await IsPageBlocked(subject.Id, group.Id)) return RedirectToAction("Index"); //error page is watched by transfer

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

                if ((grade_for_checkM1 is null || grade_for_checkM1.State == GradeState.Unlocked || user.IsAdmin) 
                    && (grade.M1 is null || (grade.M1.Value >= 25 && grade.M1.Value <= 100)))
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
                        if (user.IsAdmin && (grade_for_checkM1 is not null) && (grade_for_checkM1.State != GradeState.Unlocked))
                        {
                            grades.Last().State = grade_for_checkM1.State;
                        }
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
                if ((grade_for_checkM2 is null || grade_for_checkM2.State == GradeState.Unlocked || user.IsAdmin)
                    && (grade.M2 is null || (grade.M2.Value >= 25 && grade.M2.Value <= 100)))
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
                        if(user.IsAdmin && (grade_for_checkM2 is not null) && (grade_for_checkM2.State != GradeState.Unlocked))
                        {
                            grades.Last().State = grade_for_checkM2.State;
                        }
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
