
namespace GradeBook.Storage.Entities
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;
    }
}
