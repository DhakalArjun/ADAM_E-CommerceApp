using ADAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADAM.DataAccess.Repository.IRepository
{
    public interface IOrderHeaderRepository : IRepository<OrderHeader>
    {
        void Update(OrderHeader obj);
        void UpdateStatus(int orderId, string orderStatus, string? paymentStatus=null);
		void UpdateStripePaymentID(int orderId, string sessionId, string paymentIntentId);
	}
}
