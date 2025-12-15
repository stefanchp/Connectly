using System.ComponentModel.DataAnnotations;

namespace connectly.Models;

public class GroupListItemViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string? MembershipStatus { get; set; }
    public bool IsModerator { get; set; }
}

public class GroupDetailsViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string? CurrentUserId { get; set; }

    public GroupMemberStatus? MembershipStatus { get; set; }
    public GroupRole? Role { get; set; }
    public bool IsModerator => Role == GroupRole.Moderator && MembershipStatus == GroupMemberStatus.Accepted;
    public bool IsMember => MembershipStatus == GroupMemberStatus.Accepted;

    public List<GroupMemberView> Members { get; set; } = new();
    public List<GroupMemberView> PendingMembers { get; set; } = new();
    public List<GroupMessageView> Messages { get; set; } = new();
}

public class GroupMemberView
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public GroupRole Role { get; set; }
    public GroupMemberStatus Status { get; set; }
}

public class GroupMessageView
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class GroupCreateViewModel
{
    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;
}
