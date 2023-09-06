using ADAM_Razor_Temp.Data;
using ADAM_Razor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADAM_Razor_Temp.Pages.Categories
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; }
        public void OnGet(int? id)
        {
            if (id != null && id != 0)
            {
                Category = _context.Categories.Find(id);
            }
        }

        public IActionResult OnPost()
        {            
            Category? obj = _context.Categories.Find(Category.CategoryId);  //we will get CategoryId from Category due to BindProperty
            if(obj != null)
            {
                _context.Categories.Remove(obj);
                _context.SaveChanges();
                TempData["successMsg"] = "Category deleted successfully !";
                return RedirectToPage("Index");
            } 
            return Page();
        }
    }
}
