using ADAM_Razor_Temp.Data;
using ADAM_Razor_Temp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADAM_Razor_Temp.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Category Category { get; set; }
        public void OnGet(int? id)
        {
            if(id != null && id !=0) {
                Category = _context.Categories.Find(id);
            }
        }

        public IActionResult OnPost()
        {
            if(ModelState.IsValid)
            {
                _context.Categories.Update(Category);
                _context.SaveChanges();
                TempData["successMsg"] = "Category edited successfully !";
                return RedirectToPage("Index");

            }
            return Page();
            
        }
    }
}
