namespace GradeBook.Storage.Entities
{
    public class Grade
    {
        public int Id { get; set; }
        public int Value { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public bool IsLocked { get; set; } = false;
    }
}
