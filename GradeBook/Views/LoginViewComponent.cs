using GradeBook.Storage.Entities;
using GradeBook.Storage;
using Microsoft.AspNetCore.Mvc;

namespace GradeBook.Views
{
    public class LoginViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("LoginView");
        }

    }
}
