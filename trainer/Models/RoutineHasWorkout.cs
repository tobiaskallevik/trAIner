namespace trainer.Models;

public class RoutineHasWorkout
{
    public int Id { get; set; }
    public int WorkoutId { get; set; }
    public Workout Workout { get; set; }
    public int RoutineId { get; set; } 
    public Routine Routine { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
}