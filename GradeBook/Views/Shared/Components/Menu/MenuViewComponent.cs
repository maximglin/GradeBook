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
                //new Models.MenuItem("Home", "Index", "Главная"),
            };

            if(User.Identity is not null && User.Identity.IsAuthenticated)
            {
                model.AddRange(
                    new List<Models.MenuItem>()
                    {
                        new Models.MenuItem("Grades", "Index", "Проставление оценок")
                    });


                var user = _context.Users.FirstOrDefault(u => u.Login == User.Identity.Name);
                if (user?.IsAdminOrGradeTransfer ?? false)
                    model.AddRange(
                         new List<Models.MenuItem>()
                         {
                            new Models.MenuItem("GradesTransfer", "Index", "Перенос оценок")
                         });

                if (user?.IsAdmin ?? false)
                {
                    model.AddRange(
                        new List<Models.MenuItem>()
                        {
                        new Models.MenuItem("Users", "Index", "Пользователи"),
                        new Models.MenuItem("Groups", "Index", "Группы"),
                        new Models.MenuItem("Subjects", "Index", "Предметы"),
                        new Models.MenuItem("Teachers", "Index", "Преподаватели")
                        });
                }
            }

            return View("MenuView", model);
        }

    }
}
