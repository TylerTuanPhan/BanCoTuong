using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Libs.Entity;

namespace Libs
{
    public class ApplicationDbContext: IdentityDbContext
    { 
        public DbSet<Room> Room { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 

        }

    }
}
