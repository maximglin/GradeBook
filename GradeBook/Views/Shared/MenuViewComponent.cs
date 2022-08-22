using GradeBook.Storage;
using Microsoft.AspNetCore.Mvc;

namespace GradeBook.Views.Shared
{
    public class MenuViewComponent : ViewComponent
    {

        public IViewComponentResult Invoke()
        {
            var model = new List<Models.MenuItem>()
            {
                new Models.MenuItem("Home", "Index", "Главная"),
                new Models.MenuItem("Home", "Privacy", "Приватность")

            };
            return View("MenuView", model);
        }

    }
}
