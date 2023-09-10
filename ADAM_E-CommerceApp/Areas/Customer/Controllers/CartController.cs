using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ADAM_E_CommerceApp.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public ShoppingCartVM shoppingCartVM { get; set; }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM cartVM = new ()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties:"Product")
            };
           
            foreach (var cart in cartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                cartVM.OrderTotal += cart.Price * cart.Count;
            }   
            return View(cartVM);
        }

        public IActionResult Summary()
        {
            return View();
        }



        public IActionResult Plus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.GetDetails(u => u.ShoppingCartId == cartId);
            if (cartFromDb.Count < 1000)
            {
                cartFromDb.Count++;
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                TempData["errorMsg"] = "Maximum 1000 count per product!";
            }            
            return RedirectToAction("Index");
        }

        public IActionResult Minus(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.GetDetails(u => u.ShoppingCartId == cartId);
            if(cartFromDb.Count > 1) {
                cartFromDb.Count--;
                _unitOfWork.shoppingCartRepository.Update(cartFromDb);
            }
            else
            {   //remove item
                _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            }
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }
        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.shoppingCartRepository.GetDetails(u => u.ShoppingCartId == cartId);
            _unitOfWork.shoppingCartRepository.Remove(cartFromDb);
            _unitOfWork.Save();
            return RedirectToAction("Index");
        }

        private double GetPriceBasedOnQuantity(ShoppingCart cart)
        {
            if(cart.Count <= 50)
            {
                return cart.Product.Price;
            }else if(cart.Count <= 100)
            {
                return cart.Product.Price50;
            }
            else
            {
                return cart.Product.Price100;
            }
        }       
    }
}
