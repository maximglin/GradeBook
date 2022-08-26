using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GradeBook.Storage.Entities
{
    public class User
    {
        public int Id { get; set; }


        public string Login { get; set; } = null!;


        public string Password { get; set; } = null!;

        public bool IsAdmin { get; set; } = false;

        public virtual List<Teacher> Teachers { get; set; } = new();

        public bool FirstLogin { get; set; } = true;
    }
}
