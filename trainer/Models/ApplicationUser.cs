using System.Collections;
using Microsoft.AspNetCore.Identity;
namespace trainer.Models;

// Class for extra user info
public class ApplicationUser : IdentityUser
{
    public string Nickname { get; set; }
    public ICollection<Workout>? Workouts { get; set; }
    public ICollection<Routine>? Routines { get; set; }
    public ICollection<LoggedWorkout>? LoggedWorkouts { get; set; }
    
    // Two navigation properties are needed since its a many to many relationship with itself
    public ICollection<Friend>? User { get; set; }
    public ICollection<Friend>? HasFriends { get; set; }
    
    public ICollection<SharedWorkout>? PendingWorkouts { get; set; }
    public ICollection<SharedRoutine>? PendingRoutines { get; set; }
}