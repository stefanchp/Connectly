using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using connectly.Data;
using connectly.Models;

namespace connectly.Controllers;

[Authorize]
public class GroupsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public GroupsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var memberships = await _context.GroupMembers.AsNoTracking()
            .Where(gm => gm.UserId == userId)
            .ToListAsync();

        var membershipLookup = memberships.ToDictionary(m => m.GroupId, m => m);

        var groups = await _context.Groups
            .Include(g => g.Members)
            .AsNoTracking()
            .OrderBy(g => g.Name)
            .ToListAsync();

        var vm = groups.Select(g =>
        {
            membershipLookup.TryGetValue(g.Id, out var mem);
            return new GroupListItemViewModel
            {
                Id = g.Id,
                Name = g.Name,
                Description = g.Description,
                MemberCount = g.Members.Count(m => m.Status == GroupMemberStatus.Accepted),
                MembershipStatus = mem?.Status.ToString(),
                IsModerator = mem?.Role == GroupRole.Moderator && mem.Status == GroupMemberStatus.Accepted
            };
        }).ToList();

        return View(vm);
    }

    public IActionResult Create()
    {
        return View(new GroupCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(GroupCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var userId = _userManager.GetUserId(User)!;

        var group = new Group
        {
            Name = model.Name.Trim(),
            Description = model.Description.Trim(),
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };

        group.Members.Add(new GroupMember
        {
            UserId = userId,
            Role = GroupRole.Moderator,
            Status = GroupMemberStatus.Accepted,
            CreatedAt = DateTime.UtcNow
        });

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Details), new { id = group.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var group = await _context.Groups
            .Include(g => g.Members)
            .Include(g => g.Messages)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        GroupMember? membership = null;
        if (userId != null)
        {
            membership = await _context.GroupMembers.AsNoTracking()
                .FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == userId);
        }

        var users = await _context.Users.AsNoTracking().ToDictionaryAsync(u => u.Id, u => u);

        var vm = new GroupDetailsViewModel
        {
            Id = group.Id,
            Name = group.Name,
            Description = group.Description,
            CreatedByName = users.TryGetValue(group.CreatedById, out var creator) ? creator.FullName : "Unknown",
            CreatedAt = group.CreatedAt,
            CurrentUserId = userId,
            MembershipStatus = membership?.Status,
            Role = membership?.Role,
            Members = group.Members
                .Where(m => m.Status == GroupMemberStatus.Accepted)
                .Select(m => new GroupMemberView
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = users.TryGetValue(m.UserId, out var u) ? u.FullName : m.UserId,
                    Role = m.Role,
                    Status = m.Status
                })
                .OrderBy(m => m.FullName)
                .ToList(),
            PendingMembers = group.Members
                .Where(m => m.Status == GroupMemberStatus.Pending)
                .Select(m => new GroupMemberView
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    FullName = users.TryGetValue(m.UserId, out var u) ? u.FullName : m.UserId,
                    Role = m.Role,
                    Status = m.Status
                })
                .ToList(),
            Messages = group.Messages
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new GroupMessageView
                {
                    Id = m.Id,
                    UserId = m.UserId,
                    AuthorName = users.TryGetValue(m.UserId, out var u) ? u.FullName : m.UserId,
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Join(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var group = await _context.Groups.FindAsync(id);
        if (group == null) return NotFound();

        var existing = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == userId);
        if (existing != null)
        {
            TempData["GroupMessage"] = "You already have a membership for this group.";
            return RedirectToAction(nameof(Details), new { id });
        }

        _context.GroupMembers.Add(new GroupMember
        {
            GroupId = id,
            UserId = userId,
            Role = GroupRole.Member,
            Status = GroupMemberStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Join request sent to moderator.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Leave(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var membership = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == userId);
        if (membership == null) return RedirectToAction(nameof(Details), new { id });

        _context.GroupMembers.Remove(membership);
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "You left the group.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveMember(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var membership = await _context.GroupMembers.FindAsync(id);
        if (membership == null) return NotFound();

        var mod = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == membership.GroupId && gm.UserId == userId && gm.Role == GroupRole.Moderator && gm.Status == GroupMemberStatus.Accepted);
        if (mod == null) return Forbid();

        membership.Status = GroupMemberStatus.Accepted;
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Member accepted.";
        return RedirectToAction(nameof(Details), new { id = membership.GroupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectMember(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var membership = await _context.GroupMembers.FindAsync(id);
        if (membership == null) return NotFound();

        var mod = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == membership.GroupId && gm.UserId == userId && gm.Role == GroupRole.Moderator && gm.Status == GroupMemberStatus.Accepted);
        if (mod == null) return Forbid();

        _context.GroupMembers.Remove(membership);
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Request declined.";
        return RedirectToAction(nameof(Details), new { id = membership.GroupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteGroup(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == id);
        if (group == null) return NotFound();

        var mod = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == id && gm.UserId == userId && gm.Role == GroupRole.Moderator && gm.Status == GroupMemberStatus.Accepted);
        if (mod == null) return Forbid();

        _context.Groups.Remove(group);
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Group deleted.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PostMessage(int groupId, string content)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["GroupMessage"] = "Message cannot be empty.";
            return RedirectToAction(nameof(Details), new { id = groupId });
        }

        var membership = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId && gm.Status == GroupMemberStatus.Accepted);
        if (membership == null) return Forbid();

        _context.GroupMessages.Add(new GroupMessage
        {
            GroupId = groupId,
            UserId = userId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = groupId });
    }

    public async Task<IActionResult> EditMessage(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var message = await _context.GroupMessages.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (message == null) return NotFound();

        var membership = await _context.GroupMembers.AsNoTracking().FirstOrDefaultAsync(gm => gm.GroupId == message.GroupId && gm.UserId == userId);
        var isMod = membership?.Role == GroupRole.Moderator && membership.Status == GroupMemberStatus.Accepted;
        if (message.UserId != userId && !isMod) return Forbid();

        return View(message);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditMessage(int id, string content)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var message = await _context.GroupMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message == null) return NotFound();

        var membership = await _context.GroupMembers.FirstOrDefaultAsync(gm => gm.GroupId == message.GroupId && gm.UserId == userId);
        var isMod = membership?.Role == GroupRole.Moderator && membership.Status == GroupMemberStatus.Accepted;
        if (message.UserId != userId && !isMod) return Forbid();

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["GroupMessage"] = "Message cannot be empty.";
            return RedirectToAction(nameof(Details), new { id = message.GroupId });
        }

        message.Content = content.Trim();
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Message updated.";
        return RedirectToAction(nameof(Details), new { id = message.GroupId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMessage(int id)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        var message = await _context.GroupMessages.FirstOrDefaultAsync(m => m.Id == id);
        if (message == null) return NotFound();

        var membership = await _context.GroupMembers.AsNoTracking().FirstOrDefaultAsync(gm => gm.GroupId == message.GroupId && gm.UserId == userId);
        var isMod = membership?.Role == GroupRole.Moderator && membership.Status == GroupMemberStatus.Accepted;
        if (message.UserId != userId && !isMod) return Forbid();

        _context.GroupMessages.Remove(message);
        await _context.SaveChangesAsync();
        TempData["GroupMessage"] = "Message deleted.";
        return RedirectToAction(nameof(Details), new { id = message.GroupId });
    }
}
