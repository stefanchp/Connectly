using System.ComponentModel.DataAnnotations;

namespace connectly.Models;

public enum GroupMemberStatus
{
    Pending = 0,
    Accepted = 1
}

public enum GroupRole
{
    Member = 0,
    Moderator = 1
}

public class Group
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string CreatedById { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
    public ICollection<GroupMessage> Messages { get; set; } = new List<GroupMessage>();
}

public class GroupMember
{
    public int Id { get; set; }
    public int GroupId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public GroupRole Role { get; set; } = GroupRole.Member;

    [Required]
    public GroupMemberStatus Status { get; set; } = GroupMemberStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Group? Group { get; set; }
}

public class GroupMessage
{
    public int Id { get; set; }
    public int GroupId { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required, StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, StringLength(1000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Group? Group { get; set; }
}
