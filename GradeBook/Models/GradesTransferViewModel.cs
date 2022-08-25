using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class GradesTransferViewModel
    {
        public Dictionary<int, (List<
(GradeBook.Storage.Entities.Group Group, string Teacher)
> Groups, Subject Subject)> Item1 { get; set; }

        public Dictionary<int, (List<
(GradeBook.Storage.Entities.Group Group, string Teacher)
> Groups, Subject Subject)> Item2 { get; set; }
    }
}
