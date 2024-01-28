using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using trainer.Data;
using trainer.Models;
using trainer.ViewModels;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class SocialController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    /*Injects the dependencies*/
    public SocialController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    // Get action to show the friends page 
    public IActionResult Friends()
    {
        // Gets the logged in user
        var user = _um.GetUserAsync(User).Result;
        
        // Gets the friends and the incoming friend requests
        var friends = _db.Friends.Where(f => f.UserId == user.Id && f.Accepted).ToList();
        var requests = _db.Friends.Where(f => f.HasFriendId == user.Id && !f.Accepted).ToList();
        
        // Creates a new view model
        var vm = new FriendsViewModel()
        {
            Friends = new List<ApplicationUser>(),
            Requests = new List<ApplicationUser>()
        };
        
        // Adds the friends to the view model
        foreach (var friend in friends)
        {
            vm.Friends.Add(_db.Users.FirstOrDefault(a => a.Id == friend.HasFriendId));
        }
        
        // Adds the requests to the view model
        foreach (var request in requests)
        {
            vm.Requests.Add(_db.Users.FirstOrDefault(a => a.Id == request.UserId));
        }
        
        // Sends the view model to the view
        return View(vm);
    }
    
    
    
    // Post action to send a friend request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendRequest(string requestedUserEmail)
    {
        
        // Gets the logged in user
        var user = _um.GetUserAsync(User).Result;
        
        // Finds the user 
        var receivingUser = _db.Users.FirstOrDefault(a => a.UserName == requestedUserEmail);

        // Gets user info if the user exists, redirects if not
        Friend alreadyFriends = null;
        Friend outGoingRequest = null;
        Friend incomingRequest = null;

        // Gets user info if the user exists, redirects if not
        if (receivingUser != null)
        {
            var friends = _db.Friends.Where(f => f.UserId == user.Id && f.HasFriendId == receivingUser.Id).ToList();

            alreadyFriends = friends.FirstOrDefault(f => f.Accepted);
            outGoingRequest = friends.FirstOrDefault(f => !f.Accepted);
            incomingRequest = _db.Friends.FirstOrDefault(f => f.UserId == receivingUser.Id && f.HasFriendId == user.Id && !f.Accepted);
        }
        else
        {
            return RedirectToAction(nameof(Friends));
        }
        
        // Redirects to the friends page if the users are already friends, if the user has already sent a request or if they try to send a request to themselves
        if (alreadyFriends != null || outGoingRequest != null || receivingUser.Id == user.Id)
        {
            return RedirectToAction(nameof(Friends));
        }
        
        // Accepts the friend request if the user has an incoming friend request from the user
        if (incomingRequest != null)
        {
            incomingRequest.Accepted = true;
            _db.Friends.Update(incomingRequest);
            
            // Creates a new friendship for the accepting user
            var newFriend = new Friend()
            {
                UserId = user.Id,
                HasFriendId = receivingUser.Id,
                Accepted = true
            };
            
            // Adds it to the db and saves the changes
            _db.Friends.Add(newFriend);
            await _db.SaveChangesAsync();
            
            return RedirectToAction(nameof(Friends));
        }
        
        // Creates a new friend request
        var newFriendRequest = new Friend()
        {
            UserId = user.Id,
            HasFriendId = receivingUser.Id,
            Accepted = false
        };
        
        // Adds it to the db and saves the changes
        _db.Friends.Add(newFriendRequest);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Friends));
    }
    
    // Post action to accept a friend request
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(string id)
    {
        // Gets the logged in user
        var user = _um.GetUserAsync(User).Result;
       
        // Sets the friend request to accepted
        var friend = _db.Friends.FirstOrDefault(f => f.UserId == id && f.HasFriendId == user.Id);
        friend.Accepted = true;
        _db.Friends.Update(friend);
        
        // Creates a new friendship for the accepting user
        var newFriend = new Friend()
        {
            UserId = user.Id,
            HasFriendId = id,
            Accepted = true
        };
        
        // Adds the friendship to the database
        _db.Friends.Add(newFriend);
        
        // Saves the changes
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Friends));
    }
    
    // Post action to delete a friend
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        // Gets the logged in user
        var user = _um.GetUserAsync(User).Result;
        
        // Deletes the friendship for the friend
        var friend2 = _db.Friends.FirstOrDefault(f => f.UserId == id && f.HasFriendId == user.Id);
        var accepted = friend2.Accepted;
        _db.Friends.Remove(friend2);
        
        // Deletes the friendship for the user
        if (accepted == true)
        {
            var friend = _db.Friends.FirstOrDefault(f => f.UserId == user.Id && f.HasFriendId == id);
            _db.Friends.Remove(friend);
        }
        
        // Saves the changes
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Friends));
    }
}