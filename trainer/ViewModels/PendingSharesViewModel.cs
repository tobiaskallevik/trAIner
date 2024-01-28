using trainer.Models;

namespace trainer.ViewModels;

public class PendingSharesViewModel
{
    public required List<SharedWorkout> Workouts { get; set; }
    public required List<SharedRoutine> Routines { get; set; }
}