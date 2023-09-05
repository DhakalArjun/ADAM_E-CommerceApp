using ADAM_E_CommerceApp.Data;
using ADAM_E_CommerceApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace ADAM_E_CommerceApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            List<Category> objCategoryList = _context.Categories.ToList();
            return View(objCategoryList);
        }
    }
}
