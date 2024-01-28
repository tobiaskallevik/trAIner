using trainer.Models;

namespace trainer.ViewModels;

public class RoutineWorkoutViewModel
{
    public IEnumerable<Routine> Routines { get; set; }
    public IEnumerable<Workout> Workouts { get; set; }
    public IEnumerable<RoutineHasWorkout> RoutineHasWorkouts { get; set; } 
}

