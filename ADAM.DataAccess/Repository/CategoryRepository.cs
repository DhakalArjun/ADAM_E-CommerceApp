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
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        /* Error here says there is no argument that corresponds to the required parameter context of a repository category.
        Well, if we go to the repository implementation here, we are saying that ApplicationDbContext will be provided to you 
        when the object will be created. Typically, when we use dependency injection, they are automatically injected, but in 
        this case, we are creating it here. So what we can say is, Hey, our category repository we want to get that ApplicationDbContext
        using dependency injection and then we can add a constructor, get that here. And when we get this implementation, we want
        to pass this implementation to all the base class. So here we can say base context. That way, whatever DbContext we get
        here  */

        private ApplicationDbContext _context;
        public CategoryRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public void Update(Category obj)
        {
            _context.Categories.Update(obj);
        }
    }
}
