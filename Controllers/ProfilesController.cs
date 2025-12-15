using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using connectly.Data;
using connectly.Models;

namespace connectly.Controllers;

[Authorize]
public class ProfilesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProfilesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Index(string? query)
    {
        var profiles = _context.Users.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLower();
            profiles = profiles.Where(u => u.FullName.ToLower().Contains(term));
        }

        var list = await profiles
            .OrderBy(u => u.FullName)
            .Select(u => new ProfileListItemViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Bio = u.Bio,
                ProfileImageUrl = u.ProfileImageUrl,
                IsPrivate = u.IsPrivate
            })
            .ToListAsync();

        return View(new ProfilesIndexViewModel { Query = query, Profiles = list });
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return NotFound();

        var profile = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (profile == null) return NotFound();

        var currentUserId = _userManager.GetUserId(User);
        var canViewFull = !profile.IsPrivate || (currentUserId != null && currentUserId == profile.Id);

        var vm = new ProfileDetailViewModel
        {
            Id = profile.Id,
            FullName = profile.FullName,
            Bio = profile.Bio,
            ProfileImageUrl = profile.ProfileImageUrl,
            IsPrivate = profile.IsPrivate,
            CanViewFull = canViewFull
        };

        return View(vm);
    }

    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        var vm = new ProfileEditViewModel
        {
            FullName = user.FullName,
            Bio = user.Bio,
            ProfileImageUrl = user.ProfileImageUrl,
            IsPrivate = user.IsPrivate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProfileEditViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Challenge();

        user.FullName = model.FullName.Trim();
        user.Bio = model.Bio.Trim();
        user.ProfileImageUrl = model.ProfileImageUrl.Trim();
        user.IsPrivate = model.IsPrivate;

        await _userManager.UpdateAsync(user);
        TempData["ProfileUpdated"] = "Profil actualizat.";

        return RedirectToAction(nameof(Details), new { id = user.Id });
    }
}
