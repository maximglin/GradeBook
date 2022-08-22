namespace GradeBook.Storage.Entities
{
    public class Subject
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual List<Group> Groups { get; set; } = new();
    }
}
