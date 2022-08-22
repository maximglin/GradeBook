using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class TeacherViewModel
    {
        public Teacher Teacher { get; set; }
        public List<TeacherSubjectGroup> SubjectGroups { get; set; }
    }
}
