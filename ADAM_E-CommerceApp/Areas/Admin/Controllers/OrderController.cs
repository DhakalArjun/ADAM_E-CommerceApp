using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using ADAM.Models.ViewModels;
using ADAM.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace ADAM_E_CommerceApp.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public OrderVM orderVM { get; set; }

		public OrderController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public IActionResult Index()
		{
			return View();
		}

		public IActionResult Details(int orderId)
		{
			orderVM = new()
			{
				OrderHeader = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderId, includeProperties: "ApplicationUser"),
				OrderDetails = _unitOfWork.orderDetailRepository.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
			};
			return View(orderVM);
		}

		[HttpPost]
		[Authorize(Roles =StaticDetails.Role_Admin+","+StaticDetails.Role_Employee)]	
        public IActionResult UpdateOrderDetails()
        {
			var orderHeaderFromDb = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);
			orderHeaderFromDb.Name=orderVM.OrderHeader.Name;
			orderHeaderFromDb.PhoneNumber=orderVM.OrderHeader.PhoneNumber;
			orderHeaderFromDb.StreetAddress=orderVM.OrderHeader.StreetAddress;
			orderHeaderFromDb.City=orderVM.OrderHeader.City;
			orderHeaderFromDb.State=orderVM.OrderHeader.State;
			orderHeaderFromDb.PostalCode=orderVM.OrderHeader.PostalCode;

			if (!string.IsNullOrEmpty(orderVM.OrderHeader.Carrier))
			{
				orderHeaderFromDb.Carrier=orderVM.OrderHeader.Carrier;
			}
            if (!string.IsNullOrEmpty(orderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
            }
			_unitOfWork.orderHeaderRepository.Update(orderHeaderFromDb);
			_unitOfWork.Save();
			TempData["successMsg"] = "Order Details Updated Successfully.";			
            return RedirectToAction(nameof(Details), new {orderId=orderHeaderFromDb.OrderHeaderId});
        }

		[HttpPost]
		[Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
		public IActionResult StartProcessing(){

            _unitOfWork.orderHeaderRepository.UpdateStatus(orderVM.OrderHeader.OrderHeaderId, StaticDetails.StatusInProcess);
			_unitOfWork.Save();
			TempData["successMsg"] = "Orders Details Updated Successfully!";
			return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);
			orderHeaderFromDb.TrackingNumber = orderVM.OrderHeader.TrackingNumber;
			orderHeaderFromDb.Carrier= orderVM.OrderHeader.Carrier;
			orderHeaderFromDb.OrderStatus = StaticDetails.StatusShipped;
			orderHeaderFromDb.ShippingDate = DateTime.Now;
			if(orderHeaderFromDb.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment)
			{
				orderHeaderFromDb.PaymentDueDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.orderHeaderRepository.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["successMsg"] = "Orders Shipped Successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.OrderHeaderId });
        }

        [HttpPost]
        [Authorize(Roles = StaticDetails.Role_Admin + "," + StaticDetails.Role_Employee)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId);
           
            if (orderHeaderFromDb.PaymentStatus == StaticDetails.PaymentStatusApproved){
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeaderFromDb.PaymentIntentId
				};
				var service = new RefundService();
				Refund refundObj = service.Create(options);
				_unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderFromDb.OrderHeaderId, StaticDetails.StatusCancelled, StaticDetails.StatusRefunded);
			}
			else{
                _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderFromDb.OrderHeaderId, StaticDetails.StatusCancelled);
            }           
            _unitOfWork.Save();
            TempData["successMsg"] = "Orders Cancelled Successfully!";
            return RedirectToAction(nameof(Details), new { orderId = orderVM.OrderHeader.OrderHeaderId });
        }

		[HttpPost]
		[ActionName("Details")]
		public IActionResult Details()
		{
			//to retrieve any missing information in orderVM from database (if any)
			orderVM.OrderHeader = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId, includeProperties: "ApplicationUser");
			orderVM.OrderDetails = _unitOfWork.orderDetailRepository.GetAll(u => u.OrderHeaderId == orderVM.OrderHeader.OrderHeaderId, includeProperties: "Product");

            //stripe logic to pay
            var domain = "https://localhost:7196/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderVM.OrderHeader.OrderHeaderId}",
                CancelUrl = domain + $"admin/order/details?orderHeaderId={orderVM.OrderHeader.OrderHeaderId}",
                //LineItems basically have all the product details
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };

            foreach (var item in orderVM.OrderDetails)
            {
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
            _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(orderVM.OrderHeader.OrderHeaderId, session.Id, session.PaymentIntentId); //till here, since payment is not started, paymentIntendId=null
            _unitOfWork.Save();
            //The url to proceed payment is in session
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303); //stripe payment page
        }        
        public IActionResult PaymentConfirmation(int orderHeaderId)

        {
            OrderHeader orderHeader = _unitOfWork.orderHeaderRepository.GetDetails(u => u.OrderHeaderId == orderHeaderId);
            if (orderHeader.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment){
                //this is order by a company
                var service = new SessionService();
                Session session = service.Get(orderHeader.SessionId);
                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.orderHeaderRepository.UpdateStripePaymentID(orderHeaderId, session.Id, session.PaymentIntentId);
                    _unitOfWork.orderHeaderRepository.UpdateStatus(orderHeaderId, orderHeader.OrderStatus, StaticDetails.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
            }           
            return View(orderHeaderId);
        }


        #region API CALLS
        [HttpGet]		
		public IActionResult GetAll(string status)
		{
			IEnumerable<OrderHeader> objOrderHeaders = _unitOfWork.orderHeaderRepository.GetAll(includeProperties:"ApplicationUser").ToList();
            if (!(User.IsInRole(StaticDetails.Role_Admin) || User.IsInRole(StaticDetails.Role_Employee)))
            {
				var claimsIdendity = (ClaimsIdentity)User.Identity;
				var userId = claimsIdendity.FindFirst(ClaimTypes.NameIdentifier).Value;
				objOrderHeaders = objOrderHeaders.Where(u => u.ApplicationUser.Id == userId).OrderByDescending(u => u.OrderDate);
				//you will not see the effect of order by due to data-table implementation which has default ascending order setting for OrderHeaderId
			}			

                switch (status)
			{
                case "pending":
					objOrderHeaders = objOrderHeaders.Where(u => u.PaymentStatus == StaticDetails.PaymentStatusDelayedPayment);                  
                    break;
                case "approved":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusApproved);
                    break;
                case "inprocess":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusInProcess);
                    break;
                case "completed":
                    objOrderHeaders = objOrderHeaders.Where(u => u.OrderStatus == StaticDetails.StatusShipped);
                    break;
                default:                    
                    break;
            }
			return Json(new { data = objOrderHeaders });
		}
		
		#endregion

	}
}
