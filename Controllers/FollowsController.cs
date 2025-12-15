using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using connectly.Data;
using connectly.Models;

namespace connectly.Controllers;

[Authorize]
public class FollowsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public FollowsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User)!;

        var incoming = await _context.FollowRequests
            .AsNoTracking()
            .Where(f => f.ToUserId == userId && f.Status == FollowRequestStatus.Pending)
            .ToListAsync();

        var followers = await _context.FollowRequests
            .AsNoTracking()
            .Where(f => f.ToUserId == userId && f.Status == FollowRequestStatus.Accepted)
            .ToListAsync();

        var following = await _context.FollowRequests
            .AsNoTracking()
            .Where(f => f.FromUserId == userId && f.Status == FollowRequestStatus.Accepted)
            .ToListAsync();

        var outgoingPending = await _context.FollowRequests
            .AsNoTracking()
            .Where(f => f.FromUserId == userId && f.Status == FollowRequestStatus.Pending)
            .ToListAsync();

        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u);

        var vm = new FollowDashboardViewModel
        {
            IncomingPending = incoming,
            Followers = followers,
            Following = following,
            OutgoingPending = outgoingPending,
            Users = users
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(string toUserId)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null) return Challenge();
        if (string.IsNullOrWhiteSpace(toUserId) || toUserId == currentUserId) return BadRequest();

        var existing = await _context.FollowRequests
            .FirstOrDefaultAsync(f => f.FromUserId == currentUserId && f.ToUserId == toUserId);

        if (existing != null)
        {
            if (existing.Status == FollowRequestStatus.Rejected)
            {
                existing.Status = FollowRequestStatus.Pending;
                existing.CreatedAt = DateTime.UtcNow;
                existing.RespondedAt = null;
            }
            else
            {
                TempData["FollowMessage"] = "Request already sent or accepted.";
                return RedirectToAction("Details", "Profiles", new { id = toUserId });
            }
        }
        else
        {
            _context.FollowRequests.Add(new FollowRequest
            {
                FromUserId = currentUserId,
                ToUserId = toUserId,
                Status = FollowRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });
        }

        await _context.SaveChangesAsync();
        TempData["FollowMessage"] = "Follow request sent.";
        return RedirectToAction("Details", "Profiles", new { id = toUserId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var request = await _context.FollowRequests.FirstOrDefaultAsync(f => f.Id == id && f.ToUserId == userId);
        if (request == null) return NotFound();

        request.Status = FollowRequestStatus.Accepted;
        request.RespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["FollowMessage"] = "Follow request accepted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var request = await _context.FollowRequests.FirstOrDefaultAsync(f => f.Id == id && f.ToUserId == userId);
        if (request == null) return NotFound();

        request.Status = FollowRequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["FollowMessage"] = "Follow request rejected.";
        return RedirectToAction(nameof(Index));
    }
}
