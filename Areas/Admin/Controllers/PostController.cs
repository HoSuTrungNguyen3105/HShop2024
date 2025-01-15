using Microsoft.AspNetCore.Mvc;

namespace HShop2024.Areas.Admin.Controllers
{
    public class PostController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
