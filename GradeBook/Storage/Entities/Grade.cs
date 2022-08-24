namespace GradeBook.Storage.Entities
{
    public enum GradeType
    {
        M1,
        M2
    }
    public enum GradeState
    {
        Unlocked,
        Set,
        NeedsApproval
    }


    public class Grade
    {
        public int Id { get; set; }

        public GradeType Type { get; set; }
        public int Value { get; set; }

        public int StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public int SubjectId { get; set; }
        public virtual Subject Subject { get; set; } = null!;

        public GradeState State { get; set; } = GradeState.Unlocked;
    }
}
