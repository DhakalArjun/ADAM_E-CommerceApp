using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ADAM.DataAccess.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        //Let T - Category
        IEnumerable<T> GetAll(string? includeProperties = null);

        // Category? selectedCategory2 = _context.Categories.FirstOrDefault(u => u.CategoryId == id);
        //    T      GetDetails         filter expression( function (T, bool)) 
        T GetDetails(Expression<Func<T, bool>> filter, string? includeProperties = null);

        void Add(T entity);

        /* void Update(T entity);
        Generally update method are generally completed and have special logic based on type of entities or partial updates
        so it better not have Update method in generic repository */
        
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);


    }
}
