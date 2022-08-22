using System.Security.Principal;

namespace GradeBook.Models
{
    public record class UserCredentials(string Login, string Password);

    public class UserIdentity : IIdentity
    {
        public string? AuthenticationType { get; set; } = "Cookies";

        public bool IsAuthenticated { get; set; } = false;

        public string? Name { get; set; } = "";
    }

}
