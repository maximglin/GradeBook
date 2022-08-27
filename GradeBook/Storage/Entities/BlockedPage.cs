namespace GradeBook.Storage.Entities
{
    public class BlockedPage
    {
        public int Id { get; set; }

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;


        public int GroupId { get; set; }
        public virtual Group Group { get; set; } = null!;


        public DateTime TimeStamp { get; set; }
    }
}
