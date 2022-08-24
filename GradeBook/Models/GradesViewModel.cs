using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class GradesIndexViewModel
    {
        
    }


    public class GradesViewModel
    {
        public Subject Subject { get; set; }
        public Group Group { get; set; }

        public List<Grade> Grades { get; set; } = new();

    }
}
