using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ADAM.DataAccess.Data;
using ADAM.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;

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
        }

        public void Add(T entity)
        {
            dbSet.Add(entity);
            //_context.Categories.Add(entity);
        }

        public T GetDetails(Expression<Func<T, bool>> filter)
        {
            IQueryable<T> query = dbSet;   //here we have complete dbSet
            query = query.Where(filter);   //apply filter
            return query.FirstOrDefault();  //return the first or default

            // Category? selectedCategory3 = _context.Categories.Where(u => u.CategoryId ==id).FirstOrDefault();
            //                                 dbSet            .Where(filter)                 .FirstOrDefault();
        }

        public IEnumerable<T> GetAll()
        {
            IQueryable<T> query = dbSet;
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
