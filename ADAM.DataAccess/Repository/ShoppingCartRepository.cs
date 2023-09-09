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
    public class ShoppingCartRepository : Repository<ShoppingCart>, IShoppingCartRepository
    {
      

        private ApplicationDbContext _context;
        public ShoppingCartRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(ShoppingCart obj)
        {
            _context.shoppingCarts.Update(obj);
        }
    }
}
