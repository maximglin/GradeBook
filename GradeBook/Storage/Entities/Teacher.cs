namespace GradeBook.Storage.Entities
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual List<User> Users { get; set; } = new();

    }

    public class TeacherSubjectGroup
    {
        public int Id { get; set; }

        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;

        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;
    }
}
