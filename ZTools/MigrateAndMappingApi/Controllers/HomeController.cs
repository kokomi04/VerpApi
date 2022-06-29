using Microsoft.AspNetCore.Mvc;

namespace MigrateAndMappingApi.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}