namespace trainer.Models;

public class LoggedWorkoutHasExercise
{
    public int Id { get; set; } 
    public int LoggedWorkoutId { get; set; } 
    public LoggedWorkout LoggedWorkout { get; set; }  
    public int ExerciseId { get; set; }
    public Exercise Exercise { get; set; }
    public int IsOrder { get; set; }
    public int Sets { get; set; }
    public int? Weight { get; set; }  
    public int? Min { get; set; } 
    public int? Reps { get; set; }
}