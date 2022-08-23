using GradeBook.Storage.Entities;

namespace GradeBook.Models
{
    public class EditGroupsViewModel
    {
        public Subject Subject { get; set; }

        public List<SelectedGroupViewModel> Groups { get; set; }
    }

    public class SelectedGroupViewModel
    {
        public Group Group { get; set; }
        public bool IsSelected { get; set; }
    }

}
