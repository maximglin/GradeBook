using GradeBook.Storage;
using Microsoft.AspNetCore.Mvc;

namespace GradeBook.Views.Shared
{
    public class MenuViewComponent : ViewComponent
    {
        IGradeBookContext _context;

        public MenuViewComponent(IGradeBookContext context)
        {
            _context = context;
        }


        public IViewComponentResult Invoke()
        {
            var model = new List<Models.MenuItem>()
            {
                new Models.MenuItem("Home", "Index", "Главная"),
            };

            if(User.Identity is not null && User.Identity.IsAuthenticated)
            {
                model.AddRange(
                    new List<Models.MenuItem>()
                    {
                        new Models.MenuItem("Home", "Privacy", "Проставление оценок")
                    });


                if (_context.Users.FirstOrDefault(u => u.Login == User.Identity.Name)?.IsAdmin ?? false)
                {
                    model.AddRange(
                        new List<Models.MenuItem>()
                        {
                        new Models.MenuItem("Home", "Index", "Перенос оценок"),
                        new Models.MenuItem("Users", "Index", "Пользователи"),
                        new Models.MenuItem("Home", "Index", "Группы"),
                        new Models.MenuItem("Home", "Index", "Преподаватели")
                        });
                }
            }

            return View("MenuView", model);
        }

    }
}
