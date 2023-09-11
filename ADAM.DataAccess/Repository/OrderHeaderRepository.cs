using ADAM.DataAccess.Data;
using ADAM.DataAccess.Repository.IRepository;
using ADAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAM.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
      

        private ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(OrderHeader obj)
        {
            _context.OrderHeaders.Update(obj);
        }

		public void UpdateStatus(int orderId, string orderStatus, string? paymentStatus = null)
		{
			var orderFromDb = _context.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderId);
            if(orderFromDb != null){
                orderFromDb.OrderStatus = orderStatus;
                if (!string.IsNullOrEmpty(paymentStatus))
                {
                    orderFromDb.PaymentStatus = paymentStatus;
                }
            }
		}

		public void UpdateStripePaymentID(int orderId, string sessionId, string paymentIntentId)
		{
			var orderFromDb = _context.OrderHeaders.FirstOrDefault(u => u.OrderHeaderId == orderId);
			if (orderFromDb !=null && !string.IsNullOrEmpty(sessionId))
			{
                orderFromDb.SessionId = sessionId;
			}
			if (orderFromDb != null && !string.IsNullOrEmpty(paymentIntentId))
			{
				orderFromDb.PaymentIntentId = paymentIntentId;
			}
		}
	}
}
