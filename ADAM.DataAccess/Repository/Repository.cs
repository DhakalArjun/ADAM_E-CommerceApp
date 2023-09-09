using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ADAM.DataAccess.Data;
using ADAM.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ADAM.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        //public DbSet<Category> Categories { get; set; }

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            this.dbSet = _context.Set<T>();
            //dbSet == _context.Categories 
            //_context.Products.Include(u => u.Category); //to include Category from foreign Key relationship
            //_context.Products.Include(u => u.Category).Include(u => u.anotherTable) - we can use multiple include together
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
            //_context.Categories.Add(entity);
        }

        //Category, CoverType etc in comma separated values
        public T GetDetails(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = false)
        {
            IQueryable<T> query;
            if (tracked) //of tracked==true
            {
                query = dbSet;   //here we have complete dbSet
            }
            else
            {
                query = dbSet.AsNoTracking();   //as not tracking -- to avoid unintentional data change and save
            }
            query = query.Where(filter);   //apply filter
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);

                }
            }
            return query.FirstOrDefault();  //return the first or default

            // Category? selectedCategory3 = _context.Categories.Where(u => u.CategoryId ==id).FirstOrDefault();
            //                                 dbSet            .Where(filter)                 .FirstOrDefault();

        }

        //Category, CoverType etc in comma separated values
        public IEnumerable<T> GetAll(string? includeProperties = null)
        //public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbSet;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(new char[] { ','}, StringSplitOptions.RemoveEmptyEntries)) 
                {
                    query = query.Include(includeProperty);

                }
            }
            return query.ToList();
        }

        public void Remove(T entity)
        {
           dbSet.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
           dbSet.RemoveRange(entities);
        }
    }
}
