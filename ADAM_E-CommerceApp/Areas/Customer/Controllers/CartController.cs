using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using ADAM.Utility;
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
        [BindProperty]
        public ShoppingCartVM cartVM { get; set; }
        public CartController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public ShoppingCartVM shoppingCartVM { get; set; }


        public IActionResult Index()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM cartVM = new()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };
           
            foreach (var cart in cartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }   
            return View(cartVM);
        }

        public IActionResult Summary()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ShoppingCartVM cartVM = new()
            {
                ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product"),
                OrderHeader = new()
            };
            cartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.GetDetails(u => u.Id == userId);

            cartVM.OrderHeader.Name = cartVM.OrderHeader.ApplicationUser.Name;
            cartVM.OrderHeader.PhoneNumber = cartVM.OrderHeader.ApplicationUser.PhoneNumber;
            cartVM.OrderHeader.StreetAddress = cartVM.OrderHeader.ApplicationUser.StreetAddress;
            cartVM.OrderHeader.City = cartVM.OrderHeader.ApplicationUser.City;
            cartVM.OrderHeader.State = cartVM.OrderHeader.ApplicationUser.State;
            cartVM.OrderHeader.PostalCode = cartVM.OrderHeader.ApplicationUser.PostalCode;

            foreach (var cart in cartVM.ShoppingCartList)
            {
                cart.Price = GetPriceBasedOnQuantity(cart);
                cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
            }
            return View(cartVM);
        }

        [HttpPost]
        [ActionName("Summary")]
        public IActionResult SummaryPost() //do not need to pass ShoppingVM as it is already bind and get populated from view using BindProperty
		{
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			cartVM.ShoppingCartList = _unitOfWork.shoppingCartRepository.GetAll(u => u.ApplicationUserId == userId, includeProperties: "Product");
            
            cartVM.OrderHeader.OrderDate = DateTime.Now;
            cartVM.OrderHeader.ApplicationUserId = userId;

            cartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.GetDetails(u => u.Id == userId);

            /* we will get this from hidden input
			foreach (var cart in cartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}
            */

            if (cartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0){
                //it is a regular customer
                cartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusPending;
                cartVM.OrderHeader.OrderStatus = StaticDetails.StatusPending;
            }
            else{
				//it a company user
				cartVM.OrderHeader.PaymentStatus = StaticDetails.PaymentStatusDelayedPayment;
				cartVM.OrderHeader.OrderStatus = StaticDetails.StatusApproved;
			}
            _unitOfWork.orderHeaderRepository.Add(cartVM.OrderHeader);
            _unitOfWork.Save();

            foreach(var cart in cartVM.ShoppingCartList){
                OrderDetail detail = new()
                {
                    ProductId = cart.ProductId,
                    OrderHeaderId = cartVM.OrderHeader.OrderHeaderId,
                    Price = cart.Price,
                    Count = cart.Count
                };
                _unitOfWork.orderDetailRepository.Add(detail);
                _unitOfWork.Save();
            }
			if (cartVM.OrderHeader.ApplicationUser.CompanyId.GetValueOrDefault() == 0)
			{
				//it is a regular customer account and we need to capture payment
                //stripe logic
			} //else go to order confirmation
			return View(cartVM);
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
