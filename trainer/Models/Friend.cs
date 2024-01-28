namespace trainer.Models;

// Friend class for many-to-many relationship between users
public class Friend
{
    // Requester
    public string UserId { get; set; }
    public ApplicationUser? User { get; set; }
    
    // Requestee
    public string HasFriendId { get; set; }
    public ApplicationUser? HasFriend { get; set; }
    
    // Bool to check if friend request is accepted or not
    public bool Accepted { get; set; }
    
    public bool IsRead { get; set; }
    
    // Defaults accepted to false
    public Friend()
    {
        Accepted = false;
    }
}
