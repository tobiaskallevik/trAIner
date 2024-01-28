namespace trainer.Models;

public class SharedRoutine
{
    public required string ReceivingUserId { get; set; }
    public int RoutineId { get; set; }
    
    public ApplicationUser? ReceivingUser;
    public Routine? PendingRoutine;
}