using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GradeBook.Storage.Entities
{
    public enum UserRole
    {
        [Display(Name = "Обычный")]
        User,
        [Display(Name = "Переставлятель")]
        GradeTransfer,
        [Display(Name = "Администратор")]
        Admin
    }

    public class User
    {
        public int Id { get; set; }


        public string Login { get; set; } = null!;


        public string Password { get; set; } = null!;

        public UserRole Role { get; set; } = UserRole.User;

        public virtual List<Teacher> Teachers { get; set; } = new();

        public bool FirstLogin { get; set; } = true;


        public bool IsAdmin => Role == UserRole.Admin;
        public bool IsGradeTransfer => Role == UserRole.GradeTransfer;
        public bool IsAdminOrGradeTransfer => Role == UserRole.GradeTransfer || Role == UserRole.Admin;
    }
}
