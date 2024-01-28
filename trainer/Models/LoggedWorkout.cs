namespace trainer.Models;

public class LoggedWorkout
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public string UserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public ICollection<LoggedWorkoutHasExercise>? LoggedWorkoutHasExercises { get; set; }
}