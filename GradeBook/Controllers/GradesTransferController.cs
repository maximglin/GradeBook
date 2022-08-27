using GradeBook.Models;
using GradeBook.Storage;
using GradeBook.Storage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;



namespace GradeBook.Controllers
{

    [Authorize(Policy = "AdminOrGradeTransfer")]
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



            var rels = await _context.TeacherSubjectGroupRelations.ToListAsync();

            var subjects = await _context.Subjects.OrderBy(s => s.Name).ToListAsync();

            var grades = await _context.Grades.ToListAsync();

            Dictionary<int, (List<(GradeBook.Storage.Entities.Group Group, string Teacher)> Groups, Subject Subject)> dic1, dic2;

            dic1 = subjects.Where(s => grades.Any(g => g.SubjectId == s.Id && (g.State == GradeState.Unlocked || g.State == GradeState.NeedsApproval)))
                .ToDictionary(s => s.Id, s => (s.Groups.OrderBy(g => g.Name)
                .Where(group => grades.Any(g => g.SubjectId == s.Id && group.Students.Contains(g.Student) && (g.State == GradeState.Unlocked || g.State == GradeState.NeedsApproval)))
                .Select(g => (g,
            string.Join(", ", rels.Where(r => r.SubjectId == s.Id && r.GroupId == g.Id).Select(r => r.Teacher.Name))
            )).ToList(), s));


            dic2 = subjects.Where(s => s.Groups.Any(group => grades.Where(g => g.SubjectId == s.Id && group.Students.Contains(g.Student)).All(g => g.State == GradeState.Set)))
                .ToDictionary(s => s.Id, s => (s.Groups.OrderBy(g => g.Name)
                .Where(group => grades.Where(g => g.SubjectId == s.Id && group.Students.Contains(g.Student)).All(g => g.State == GradeState.Set))
                .Select(g => (g,
            string.Join(", ", rels.Where(r => r.SubjectId == s.Id && r.GroupId == g.Id).Select(r => r.Teacher.Name))
            )).ToList(), s));

            


            return View(new GradesTransferViewModel()
            {
                Item1 = dic1,
                Item2 = dic2
            });
        }

        private async Task BlockPage(Subject subject, Group group)
        {
            if (subject is null || group is null) return;

            var block = await _context.BlockedPages.FirstOrDefaultAsync(b => b.SubjectId == subject.Id && b.GroupId == group.Id);
            if (block is null)
                _context.BlockedPages.Add(new BlockedPage()
                {
                    Subject = subject,
                    Group = group,
                    TimeStamp = DateTime.UtcNow
                });
            else
                block.TimeStamp = DateTime.UtcNow;

            await _context.SaveChangesAsync();
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

            await BlockPage(subject, group);

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
