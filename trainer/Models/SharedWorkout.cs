namespace trainer.Models;

public class SharedWorkout
{
    public required string ReceivingUserId { get; set; }
    public int WorkoutId { get; set; }

    public ApplicationUser? ReceivingUser;
    public Workout? PendingWorkout;
}