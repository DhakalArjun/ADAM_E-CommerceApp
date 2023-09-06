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
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Category obj)
        {
            //add custom validation check for 'Name' property here case don't matter
            if(obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("Name", "The Display Order cannot be exactly match the Name.");
            }

            //add custom validation check without any field specified //Name can't be 'test'
            if (obj.Name != null &&  obj.Name.ToLower() == "test") //obj.Name != null is added to avoid Null Pointer Exception for obj.Name.ToLower()
            {
                ModelState.AddModelError("", "Category Name cannot be 'test'.");
            }

            if (ModelState.IsValid)
            {   
                _context.Categories.Add(obj);
                _context.SaveChanges();
                //if with the same controller we don't need to write controller name below, but let it be there for being more explicit
                TempData["successMsg"] = "Category created successfully!";
                return RedirectToAction("Index", "Category");
            }
            return View();            
           
        }

        public IActionResult Edit(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            Category? selectedCategory = _context.Categories.Find(id); //this only works with primary key
           // Category? selectedCategory2 = _context.Categories.FirstOrDefault(u => u.CategoryId == id); //with any property and operator like == or .contains etc.
           // Category? selectedCategory3 = _context.Categories.Where(u => u.CategoryId ==id).FirstOrDefault();
            if (selectedCategory == null)
            {
                return NotFound();
            }
            return View(selectedCategory);
        }

        [HttpPost]
        public IActionResult Edit(Category obj)
        {  
            //custom validation on Create action method we removed as they are not practically required because they are there for educational purpose.
            if (ModelState.IsValid)
            {
                _context.Categories.Update(obj);
                _context.SaveChanges();
                TempData["successMsg"] = "Category edited successfully!";
                return RedirectToAction("Index");
            }
            return View();

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? selectedCategory = _context.Categories.Find(id);
            if (selectedCategory == null)
            {
                return NotFound();
            }
            return View(selectedCategory);
        }

        [HttpPost, ActionName("Delete")]    //ActionName("Delete") means although name of action method is something different consider it as "Delete"
        public IActionResult DeletePOST(int? id)  //here id is passed as other fields Name and DisplayOrder field are disabled, alternatively we can use readonly for those fields and pass Category obj
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? obj = _context.Categories.Find(id);
            if (obj == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _context.Categories.Remove(obj);
                _context.SaveChanges();
                TempData["successMsg"] = "Category deleted successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
