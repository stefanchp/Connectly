using System.ComponentModel.DataAnnotations;

namespace connectly.Models;

public class ProfileListItemViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
}

public class ProfilesIndexViewModel
{
    public string? Query { get; set; }
    public List<ProfileListItemViewModel> Profiles { get; set; } = new();
}

public class ProfileDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ProfileImageUrl { get; set; } = string.Empty;
    public bool IsPrivate { get; set; }
    public bool CanViewFull { get; set; }
    public bool IsSelf { get; set; }
    public bool IsFollowingAccepted { get; set; }
    public bool HasPendingRequest { get; set; }
}

public class ProfileEditViewModel
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, StringLength(280)]
    public string Bio { get; set; } = string.Empty;

    [Required, Url]
    [Display(Name = "Profile image URL")]
    public string ProfileImageUrl { get; set; } = string.Empty;

    [Display(Name = "Private profile")]
    public bool IsPrivate { get; set; }
}
