namespace trainer.Models;

public class Exercise
{
    public int Id { get; set; } 
    public string Name { get; set; } 
    public string MuscleGroup { get; set; } 
    public bool RequiresGym { get; set; } 
    public string GifUrl { get; set; }
    
    // The exercises that are part of the workout
    public ICollection<WorkoutHasExercise>? WorkoutHasExercises { get; set; }
    // The exercises that are part of the logged workout
    public ICollection<LoggedWorkoutHasExercise>? LoggedWorkoutHasExercises { get; set; }
}