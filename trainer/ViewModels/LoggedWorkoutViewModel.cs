using trainer.Models;

namespace trainer.ViewModels;

public class LoggedWorkoutViewModel
{
    public LoggedWorkout LoggedWorkout { get; set; }
    public string WorkoutName { get; set; }
    public IEnumerable<Exercise> Exercises { get; set; }
    public IEnumerable<LoggedWorkoutHasExercise> LoggedWorkoutHasExercises { get; set; }
    public IEnumerable<LoggedWorkoutHasExercise>? LastLoggedWorkoutHasExercises { get; set; }
    
}