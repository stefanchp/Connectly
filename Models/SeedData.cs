using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using connectly.Data;

namespace connectly.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

        context.Database.EnsureCreated();

        SeedRoles(context);
        SeedUsers(context);

        context.SaveChanges();
    }

    private static void SeedRoles(ApplicationDbContext context)
    {
        if (context.Roles.Any()) return;

        context.Roles.AddRange(
            new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7210", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "2c5e174e-3b0e-446f-86af-483d56fd7211", Name = "User", NormalizedName = "USER" }
        );
    }

    private static void SeedUsers(ApplicationDbContext context)
    {
        var hasher = new PasswordHasher<ApplicationUser>();

        var users = new[]
        {
            new ApplicationUser
            {
                Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                UserName = "admin@test.com",
                NormalizedUserName = "ADMIN@TEST.COM",
                Email = "admin@test.com",
                NormalizedEmail = "ADMIN@TEST.COM",
                EmailConfirmed = true,
                FullName = "Admin User",
                Bio = "Platform administrator.",
                ProfileImageUrl = "https://placehold.co/200x200?text=Admin",
                IsPrivate = false
            },
            new ApplicationUser
            {
                Id = "1d3f0c9a-85fa-4b4d-8f9e-111111111111",
                UserName = "maria@example.com",
                NormalizedUserName = "MARIA@EXAMPLE.COM",
                Email = "maria@example.com",
                NormalizedEmail = "MARIA@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Maria Ionescu",
                Bio = "Coffee lover and hobby photographer.",
                ProfileImageUrl = "https://placehold.co/200x200?text=Maria",
                IsPrivate = false
            },
            new ApplicationUser
            {
                Id = "2d3f0c9a-85fa-4b4d-8f9e-222222222222",
                UserName = "andrei@example.com",
                NormalizedUserName = "ANDREI@EXAMPLE.COM",
                Email = "andrei@example.com",
                NormalizedEmail = "ANDREI@EXAMPLE.COM",
                EmailConfirmed = true,
                FullName = "Andrei Popescu",
                Bio = "Trail runner and backend dev.",
                ProfileImageUrl = "https://placehold.co/200x200?text=Andrei",
                IsPrivate = true
            }
        };

        var passwordById = new Dictionary<string, string>
        {
            ["8e445865-a24d-4543-a6c6-9443d048cdb0"] = "Admin1!",
            ["1d3f0c9a-85fa-4b4d-8f9e-111111111111"] = "User1!",
            ["2d3f0c9a-85fa-4b4d-8f9e-222222222222"] = "User2!"
        };

        foreach (var user in users)
        {
            if (context.Users.Any(u => u.Id == user.Id)) continue;
            user.PasswordHash = hasher.HashPassword(user, passwordById[user.Id]);
            context.Users.Add(user);
        }

        // Assign roles if not already assigned
        var userRoles = new[]
        {
            new IdentityUserRole<string>
            {
                UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210" // Admin
            },
            new IdentityUserRole<string>
            {
                UserId = "1d3f0c9a-85fa-4b4d-8f9e-111111111111",
                RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211" // User
            },
            new IdentityUserRole<string>
            {
                UserId = "2d3f0c9a-85fa-4b4d-8f9e-222222222222",
                RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211" // User
            }
        };

        foreach (var ur in userRoles)
        {
            var exists = context.UserRoles.Any(x => x.UserId == ur.UserId && x.RoleId == ur.RoleId);
            if (!exists) context.UserRoles.Add(ur);
        }
    }
}
