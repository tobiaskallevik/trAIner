using trainer.Models;

namespace trainer.ViewModels;

// View model for creating a workout
public class FriendsViewModel
{
    public List<ApplicationUser> Friends { get; set; }
    public List<ApplicationUser> Requests { get; set; }
}

