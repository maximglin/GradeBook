namespace GradeBook.Models
{
    public class ChangePasswordViewModel
    {
        public string Login { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordRepeat { get; set; }
        public bool FirstChange { get; set; }
    }
}
