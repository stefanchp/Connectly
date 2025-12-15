using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; // Necesar pentru DbContextOptions
using Microsoft.Extensions.DependencyInjection; // Necesar pentru GetRequiredService
using connectly.Data;

namespace connectly.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
                serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificăm dacă există deja roluri în baza de date [cite: 12085]
                if (context.Roles.Any())
                {
                    return;   // Baza de date a fost deja inițializată
                }

                // CREAREA ROLURILOR [cite: 12093]
                context.Roles.AddRange(
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "ADMIN" },
                    new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "User", NormalizedName = "USER" }
                );

                // CREAREA UNUI USER ADMIN DEFAULT [cite: 12108]
                var hasher = new PasswordHasher<ApplicationUser>();

                context.Users.AddRange(
                    new ApplicationUser
                    {
                        Id = "8e445865-a24d-4543-a6c6-9443d048cdb0", // Primary Key
                        UserName = "admin@test.com",
                        NormalizedUserName = "ADMIN@TEST.COM",
                        Email = "admin@test.com",
                        NormalizedEmail = "ADMIN@TEST.COM",
                        EmailConfirmed = true,
                        PasswordHash = hasher.HashPassword(null, "Admin1!")
                    }
                );

                // ASOCIEREA USER-ROLE (Admin user primește rolul de Admin) [cite: 12148]
                context.UserRoles.AddRange(
                    new IdentityUserRole<string>
                    {
                        RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210", // ID-ul rolului Admin
                        UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"  // ID-ul userului Admin
                    }
                );

                context.SaveChanges();
            }
        }
    }
}