using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class EditStudentsViewModel
    {
        public Group Group { get; set; }
        public string StudentsToAdd { get; set; }
        public List<Student> Students { get; set; } = new();
    }

}
