using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class EditTeacherSubjectsViewModel
    {
        public Teacher Teacher { get; set; }
        public Dictionary<int, List<SelectedSubjectGroupViewModel>> Selected { get; set; } = new();

    }
    public class SelectedSubjectGroupViewModel
    {
        public Subject Subject { get; set; }
        public Group Group { get; set; }
        public bool IsSelected { get; set; }
    }
}
