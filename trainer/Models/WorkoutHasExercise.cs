using System.ComponentModel.DataAnnotations;
namespace trainer.Models;

public class WorkoutHasExercise
{
    // Composite key
    public int WorkoutId { get; set; } // Foreign key
    public Workout Workout { get; set; }
    public int ExerciseId { get; set; } // Foreign key
    public Exercise Exercise { get; set; }
    public int IsOrder { get; set; }
    public int Sets { get; set; }
}