using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ADAM_E_CommerceApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            List<Product> objProductList = _unitOfWork.productRepository.GetAll(includeProperties:"Category").ToList();            
            return View(objProductList);
        }
        public IActionResult Upsert(int? id)  //Uspert => Update + Insert (create)
        //public IActionResult Create()
        {
            /*
            IEnumerable<SelectListItem> categoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.CategoryId.ToString()
            });*/
            //ViewBag.CategoryList = categoryList;
            /* alternative of above ViewBag insertion
            List<Category> categoryList = _unitOfWork.categoryRepository.GetAll().ToList();
            ViewBag.CategoryList = new SelectList(categoryList, "CategoryId", "Name");
            */
            ProductVM productVM = new ProductVM()
            {
                CategoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
                { //insert/create
                    Text = u.Name,
                    Value = u.CategoryId.ToString()
                }),
            Product = new Product()
            };
            if(id !=null || id != 0) //Update case
            { 
                productVM.Product = _unitOfWork.productRepository.GetDetails(u => u.ProductId == id);               
            }
            return View(productVM);            
        }

        [HttpPost]
        public IActionResult Upsert(ProductVM productVM, IFormFile? file)
        {            
            if (ModelState.IsValid)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                if(file != null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string productPath = Path.Combine(wwwRootPath, @"images\product");
                    //delete Old file while updating
                    if (!string.IsNullOrEmpty(productVM.Product.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(wwwRootPath, productVM.Product.ImageUrl.TrimStart('\\'));
                        if(System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }
                    //copy new file
                    using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                    {
                        file.CopyTo(fileStream);
                    }
                    productVM.Product.ImageUrl = @"\images\product\" + fileName;
                }
                if (productVM.Product.ProductId == 0)
                {
                    _unitOfWork.productRepository.Add(productVM.Product);
                }
                else
                {
                    _unitOfWork.productRepository.Update(productVM.Product);
                }                
                _unitOfWork.Save();             
                TempData["successMsg"] = "Product created successfully!";
                return RedirectToAction("Index", "Product");
            }            
            productVM.CategoryList = _unitOfWork.categoryRepository.GetAll().Select(u => new SelectListItem
            {
                Text = u.Name,
                Value = u.CategoryId.ToString()
            });           
            return View(productVM);
        }
        /*

        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }

            Product? selectedProduct = _unitOfWork.productRepository.GetDetails(u => u.ProductId == id);           
            if (selectedProduct == null)
            {
                return NotFound();
            }
            return View(selectedProduct);
        }

        [HttpPost]
        public IActionResult Edit(Product obj)
        {          
            if (ModelState.IsValid)
            {
                _unitOfWork.productRepository.Update(obj);
                _unitOfWork.Save();               
                TempData["successMsg"] = "Product edited successfully!";
                return RedirectToAction("Index");
            }
            return View();

        }
        */
        /*
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? selectedProduct = _unitOfWork.productRepository.GetDetails(u => u.ProductId == id);  
            if (selectedProduct == null)
            {
                return NotFound();
            }
            return View(selectedProduct);
        }

        [HttpPost, ActionName("Delete")]   
        public IActionResult DeletePOST(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Product? obj = _unitOfWork.productRepository.GetDetails(u => u.ProductId == id);            
            if (obj == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                _unitOfWork.productRepository.Remove(obj);
                _unitOfWork.Save();               
                TempData["successMsg"] = "Product deleted successfully!";
                return RedirectToAction("Index");
            }
            return View();
        }
        */

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Product> objProductList = _unitOfWork.productRepository.GetAll(includeProperties:"Category").ToList();
            return Json(new {data = objProductList});
        }

        [HttpDelete]
        public IActionResult Delete (int? id)
        {
            Product? projectToBeDeleted = _unitOfWork.productRepository.GetDetails(u => u.ProductId==id);
            if(projectToBeDeleted == null)
            {
                return Json(new { success = false, message = "Error while deleting" });
            }
            //if product, also delete images
            var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, projectToBeDeleted.ImageUrl.TrimStart('\\'));
            if(System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            _unitOfWork.productRepository.Remove(projectToBeDeleted);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Product deleted successfully" });
        }
        #endregion
    }
}
