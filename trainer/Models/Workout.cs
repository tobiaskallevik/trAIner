using System.ComponentModel.DataAnnotations;

namespace trainer.Models;

// Class for workouts. Each workout can have 0 or 1 user and 0, 1 or many exercises
public class Workout
{
    
    public int Id { get; set; } 
    public string? UserId { get; set; } //Foreign key
    
    // Required, alphanumeric, max 20 characters
    [Required]
    [StringLength(20)]
    [RegularExpression("^[a-zA-Z0-9æøåÆØÅ ]*$", ErrorMessage = "Only alphanumeric characters are allowed.")]
    public string Name { get; set; } 
    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<WorkoutHasExercise>? WorkoutHasExercises { get; set; }

    public ICollection<RoutineHasWorkout>? RoutineHasWorkouts { get; set; }
    public ICollection<LoggedWorkout>? LoggedWorkouts { get; set; }
    
    public ICollection<SharedWorkout>? ReceivingUsers { get; set; }
}