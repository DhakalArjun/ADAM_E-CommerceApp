using ADAM_Razor_Temp.Data;
using ADAM_Razor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADAM_Razor_Temp.Pages.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Category> CategoryList { get; set; }    

        public void OnGet()
        {
            CategoryList = _context.Categories.ToList();
        }
    }
}
