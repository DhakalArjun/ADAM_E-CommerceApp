using Microsoft.AspNetCore.Mvc;

namespace ADAM_E_CommerceApp.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
