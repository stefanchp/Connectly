using System.ComponentModel.DataAnnotations;

namespace connectly.Models;

public enum FollowRequestStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class FollowRequest
{
    public int Id { get; set; }

    [Required]
    public string FromUserId { get; set; } = string.Empty;

    [Required]
    public string ToUserId { get; set; } = string.Empty;

    [Required]
    public FollowRequestStatus Status { get; set; } = FollowRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }
}
