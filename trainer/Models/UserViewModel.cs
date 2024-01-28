namespace trainer.Models;

public class UserViewModel
{
    public ApplicationUser User { get; set; }
    public bool IsAdmin { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsBanned { get; set; }
}