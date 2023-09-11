using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using ADAM.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
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

            //cartVM.OrderHeader.ApplicationUser = _unitOfWork.applicationUserRepository.GetDetails(u => u.Id == userId);

			/* What I did in above line of code is when we were creating the post action method I populated application user. Now typically 
			what happens is when you are populating a record here and you are telling entity framework code that, hey,
            I want to add the order header, it will also add all the corresponding navigation properties. It will think
            that okay you are trying to create a new entity. If you do not want that then you should never populate a 
            navigation property when you are trying to insert a record in Entity Framework Core, always remember that.
            So work around to this particular issue is we can have a new application user, call that application user,
            and then right here we will access that application user also in this if condition. So that is one thing that 
            you should always remember when you see that Entity Framework Core is trying to add a navigation property,
            most likely the navigation property is populated. alternative do following*/

            ApplicationUser applicationUser = _unitOfWork.applicationUserRepository.GetDetails(u => u.Id == userId);
            			
			foreach (var cart in cartVM.ShoppingCartList)
			{
				cart.Price = GetPriceBasedOnQuantity(cart);
				cartVM.OrderHeader.OrderTotal += cart.Price * cart.Count;
			}           

			if (applicationUser.CompanyId.GetValueOrDefault() == 0){
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
			if (applicationUser.CompanyId.GetValueOrDefault() == 0)
			{
                //it is a regular customer account and we need to capture payment //stripe logic here
                var domain = "https://localhost:7196/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={cartVM.OrderHeader.OrderHeaderId}",
                    CancelUrl = domain + "customer/cart/index",
                    //LineItems basically have all the product details
					LineItems = new List<SessionLineItemOptions>(),  
					Mode = "payment",
				};

                foreach(var item in cartVM.ShoppingCartList){
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100), //$20.50 => 2050
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Title
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                //creating new session service
				var service = new SessionService();
				Session session = service.Create(options);
                _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(cartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId); //till here, since payment is not started, paymentIntendId=null
                _unitOfWork.Save();
                //The url to proceed payment is in session
                Response.Headers.Add("Location", session.Url);
                return new StatusCodeResult(303); //stripe payment page

			} //else go to order confirmation
            int orderId = cartVM.OrderHeader.OrderHeaderId;
			return View(nameof(OrderConfirmation),orderId);
		}

        public IActionResult OrderConfirmation(int id)
            
        {
            OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == id, includeProperties: "ApplicationUser");
            if(orderHeader.PaymentStatus != StaticDetails.PaymentStatusDelayedPayment){
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if(session.PaymentStatus.ToLower() == "paid"){
                    _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(cartVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeaderRepository.UpdateStatus(id, StaticDetails.StatusApproved, StaticDetails.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }
            List<ShoppingCart> shoppingCarts = _unitOfWork.shoppingCartRepository
                .GetAll(u => u.ApplicationUserId == orderHeader.ApplicationUserId).ToList();
            _unitOfWork.shoppingCartRepository.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
			return View(id);
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
