using Microsoft.AspNetCore.Identity;

namespace connectly.Models // Asigură-te că namespace-ul este corect
{
    public class ApplicationUser : IdentityUser
    {
        // Aici putem adăuga proprietăți extra în viitor (ex: FirstName, LastName)
        // Conform Curs 9, aceasta extinde funcționalitatea IdentityUser[cite: 1794].
    }
}