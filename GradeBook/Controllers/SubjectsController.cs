using Microsoft.AspNetCore.Mvc;

namespace GradeBook.Controllers
{
    public class SubjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
