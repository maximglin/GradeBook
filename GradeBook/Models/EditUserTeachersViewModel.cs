namespace GradeBook.Models
{
    public class EditUserTeachersViewModel
    {
        public int UserId { get; set; }
        public List<SelectedTeacherViewModel> Teachers { get; set; } = new();
    }

    public class SelectedTeacherViewModel
    {
        public GradeBook.Storage.Entities.Teacher Teacher { get; set; }
        public bool Selected { get; set; } = false;
    }
}
