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
    }
}
