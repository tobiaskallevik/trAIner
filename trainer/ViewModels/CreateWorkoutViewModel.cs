using trainer.Models;

namespace trainer.ViewModels;

// View model for creating a workout
public class CreateWorkoutViewModel
{
    public Workout Workout { get; set; }
    public List<Workout> Workouts { get; set; }
    public List<Exercise> Exercises { get; set; }
    public string UserId { get; set; }
}
