using System.ComponentModel.DataAnnotations;
namespace trainer.Models;

public class Routine
{
    public int Id { get; set; } 
    public string? UserId { get; set; } 
    
    [Required]
    [StringLength(20)]
    [RegularExpression("^[a-zA-Z0-9æøåÆØÅ ]*$", ErrorMessage = "Only alphanumeric characters are allowed.")]
    public string Name { get; set; } 
    
    public ApplicationUser? ApplicationUser { get; set; }
    public ICollection<RoutineHasWorkout>? RoutineHasWorkouts { get; set; }
    public ICollection<SharedRoutine>? ReceivingUsers { get; set; }
    
}