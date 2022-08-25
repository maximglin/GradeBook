using GradeBook.Storage.Entities;

namespace GradeBook.Models
{

    public class GradesViewModel
    {
        public Subject Subject { get; set; }
        public Group Group { get; set; }

        public List<GradeViewModel> Grades { get; set; } = new();

        public bool IsAdminEditing { get; set; } = false;

    }


    
    public class GradeViewModel
    {
        public Student Student { get; set; }
        public int? M1 { get; set; }
        public int? M2 { get; set; }

        public GradeState? M1State { get; set; }
        public GradeState? M2State { get; set; }
    }
}
