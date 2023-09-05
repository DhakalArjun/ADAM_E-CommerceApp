using ADAM_E_CommerceApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ADAM_E_CommerceApp.Data
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :base(options)
        {            
        }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>().HasData(
                new Category { CategoryId = 1, Name = "Electronics", DisplayOrder = 1 },               
                new Category { CategoryId = 2, Name = "Books", DisplayOrder = 2 },
                new Category { CategoryId = 3, Name = "Fashion", DisplayOrder = 3 },
                new Category { CategoryId = 4, Name = "Sports & Outdoors", DisplayOrder = 4},
                new Category { CategoryId = 5, Name = "Health & Household", DisplayOrder = 5 },
                new Category { CategoryId = 6, Name = "Computers", DisplayOrder = 6 },
                new Category { CategoryId = 7, Name = "Toys & Games", DisplayOrder = 7 }
                );
        }
    }
}
