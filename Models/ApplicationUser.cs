using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace connectly.Models // Asigură-te că namespace-ul este corect
{
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, StringLength(280)]
        public string Bio { get; set; } = string.Empty;

        [Required, Url]
        public string ProfileImageUrl { get; set; } = string.Empty;

        // When true, only limited info is shown to other users.
        public bool IsPrivate { get; set; }
    }
}
