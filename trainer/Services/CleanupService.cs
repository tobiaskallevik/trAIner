using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Services;

/*Cleanup service for db cleanup etc...*/
public class CleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CleanupService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    // Deletes logged workouts that are older than 5 hours and have no exercises. Done to prevent the database from containing unnecessary data
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var cutoffTime = DateTime.Now.AddHours(-5);
                
                // Finds the workouts to delete
                var workoutsToDelete = db.LoggedWorkouts
                    .Where(w => !w.LoggedWorkoutHasExercises.Any() 
                                || (w.Date < cutoffTime 
                                    && w.LoggedWorkoutHasExercises.All(e => e.Weight == null && e.Min == null && e.Reps == null)))
                    .ToList();

                
                // Removes the logged workouts from the database
                db.LoggedWorkouts.RemoveRange(workoutsToDelete);
                await db.SaveChangesAsync();
            }

            // Waits for an hour before running again
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); 
        }
    }

}
