using System.Collections.ObjectModel;

namespace connectly.Models;

public class FollowDashboardViewModel
{
    public List<FollowRequest> IncomingPending { get; set; } = new();
    public List<FollowRequest> Followers { get; set; } = new();
    public List<FollowRequest> Following { get; set; } = new();
    public List<FollowRequest> OutgoingPending { get; set; } = new();
    public IReadOnlyDictionary<string, ApplicationUser> Users { get; set; } = new ReadOnlyDictionary<string, ApplicationUser>(new Dictionary<string, ApplicationUser>());
}
