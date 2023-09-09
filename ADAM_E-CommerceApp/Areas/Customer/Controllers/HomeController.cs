using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace ADAM_E_CommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Product> productList = _unitOfWork.productRepository.GetAll().ToList();
            return View(productList);
        }

        public IActionResult Details(int productId)
        {
            ShoppingCart cart = new()
            {
                Product = _unitOfWork.productRepository.GetDetails(u => u.ProductId == productId, includeProperties: "Category"),
                Count = 1, //default count
                ProductId = productId
            };           
            return View(cart);
        }

        [HttpPost]
        [Authorize]
        public IActionResult Details(ShoppingCart cart)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            cart.ApplicationUserId = userId;

            ShoppingCart cartFromDb = _unitOfWork.shoppingCartRepository.GetDetails(u => u.ApplicationUserId == userId && u.ProductId == cart.ProductId);
            if(cartFromDb != null)
            {
                cartFromDb.Count += cart.Count;
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            }
            else
            {
                _unitOfWork.shoppingCartRepository.Add(cart);                
            }
            TempData["successMsg"] = "Cart updated successfully";
            _unitOfWork.Save();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
