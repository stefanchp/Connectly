using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using connectly.Models;

namespace connectly.Data
{ 
    // Moștenește IdentityDbContext<ApplicationUser> conform Curs 9 
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Aici vei adăuga DbSet-urile pentru Postari, Comentarii etc. mai târziu.
    }
}