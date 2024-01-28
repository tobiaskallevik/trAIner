using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using trainer.Data;
using trainer.Models;
using trainer.ViewModels;
using Index = Microsoft.EntityFrameworkCore.Metadata.Internal.Index;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class PendingSharesController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    public PendingSharesController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
   
    // Action method to manage and display shared routines and workouts for the logged in user
    public async Task<IActionResult> Manage()
    {
        // Gets the logged in user
        var user = await _um.GetUserAsync(User);
        
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        // Fetches the shared routines and workouts associated with the user
        var routines = await _db.SharedRoutines
            .Where(sr => sr.ReceivingUserId == user.Id)
            .Include(sr => sr.PendingRoutine)
                .ThenInclude(r => r.ApplicationUser)
            .ToListAsync();
        var workouts = await _db.SharedWorkouts
            .Where(sw => sw.ReceivingUserId == user.Id)
            .Include(sw => sw.PendingWorkout)
                .ThenInclude(w => w.WorkoutHasExercises)
                    .ThenInclude(whe => whe.Exercise)
            .Include(sw => sw.PendingWorkout)
                .ThenInclude(w => w.ApplicationUser)
            .ToListAsync();

        // Prepare the ViewModel with the fetched routines and workouts
        var vm = new PendingSharesViewModel()
        {
            Workouts = workouts,
            Routines = routines
        };
        
        return View(vm);
    }
    
    // Post action to accept a shared workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptWorkout(int wid)
    {
        // Retrieves the currently logged in user and the specific shared workout with the exercises
        var user = await _um.GetUserAsync(User);
        var workout = await _db.SharedWorkouts
            .Where(sw => sw.WorkoutId == wid && sw.ReceivingUserId == user.Id)
            .Include(sw => sw.PendingWorkout)
            .ThenInclude(whe => whe.WorkoutHasExercises)
            .FirstOrDefaultAsync();

        // Gets a list of the names of all workouts connected to the user
        var workoutNamesList = await _db.Workouts
            .Where(w => w.UserId == user.Id)
            .Select(w => w.Name)
            .ToListAsync();

        // Puts a number after the workoutname if it exists
        var newName = workout.PendingWorkout.Name;
        if (workoutNamesList.Contains(newName))
        {
            int count = workoutNamesList.Count(n => n.StartsWith(newName));
            newName += count.ToString();
        }
        // Create a new workout instance copying the name, associating with the current user, and initializing an empty list for exercises
        var copiedWorkout = new Workout()
        {
            UserId = user.Id,
            Name = newName,
            WorkoutHasExercises = new List<WorkoutHasExercise>(),
            ApplicationUser = user
        };
        
        _db.Workouts.Add(copiedWorkout);
        await _db.SaveChangesAsync();
       
        // Create a copy of the workout exercise with the new workout's ID and the same exercise details
        foreach (var exercise in workout.PendingWorkout.WorkoutHasExercises)
        {
            var copiedExercise = new WorkoutHasExercise()
            {
                WorkoutId = copiedWorkout.Id,
                ExerciseId = exercise.ExerciseId,
                IsOrder = exercise.IsOrder,
                Sets = exercise.Sets
            };
            copiedWorkout.WorkoutHasExercises.Add(copiedExercise);
        }

        _db.SharedWorkouts.Remove(workout);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    // Post action to decline a shared workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclineWorkout(int wid)
    {
        // Gets the logged in user
        var user = await _um.GetUserAsync(User);
        
        // Retrieve the workout shared with the current user by the workout ID
       var pending = await _db.SharedWorkouts
           .Where(sw => sw.WorkoutId == wid && sw.ReceivingUserId == user.Id)
           .FirstOrDefaultAsync();

       if (pending == null) return NotFound();

       _db.SharedWorkouts.Remove(pending);
       await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    
    // Post action to accept a shared routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AcceptRoutine(int rid)
    {
        // Retrieves the currently logged in user and the specific shared routine with the workouts and exercises
        var user = await _um.GetUserAsync(User);
        
        var routine = await _db.SharedRoutines
            .Where(sr => sr.RoutineId == rid && sr.ReceivingUserId == user.Id)
            .Include(sr => sr.PendingRoutine)
            .ThenInclude(r => r.RoutineHasWorkouts)
            .ThenInclude(rhw => rhw.Workout)
            .ThenInclude(w => w.WorkoutHasExercises)
            .ThenInclude(whe => whe.Exercise)
            .FirstOrDefaultAsync();

        // Gets a list of the names of all routines connected to the user
        var routineNamesList = await _db.Routines
            .Where(r => r.UserId == user.Id)
            .Select(r => r.Name)
            .ToListAsync();
        
        // Gets a list of the names of all workouts connected to the user
        var workoutNamesList = await _db.Workouts
            .Where(w => w.UserId == user.Id)
            .Select(w => w.Name)
            .ToListAsync();

        // Puts a number after the routine name if it exists
        var newRoutineName = routine.PendingRoutine.Name;
        if (routineNamesList.Contains(newRoutineName))
        {
            int count = routineNamesList.Count(n => n.StartsWith(newRoutineName));
            newRoutineName += count.ToString();
        }
        // Create a new routine instance copying the name, associating with the current user, and initializing an empty list for workouts
        var copiedRoutine = new Routine()
        {
            UserId = user.Id,
            Name = newRoutineName,
            RoutineHasWorkouts = new List<RoutineHasWorkout>(),
            ApplicationUser = user
        };
        
        _db.Routines.Add(copiedRoutine);
        await _db.SaveChangesAsync();

        foreach (var routineWorkout in routine.PendingRoutine.RoutineHasWorkouts)
        {
            var originalWorkout = routineWorkout.Workout;
            var newWorkoutName = originalWorkout.Name;

            // Checks the workout name and change if necessary
            if (workoutNamesList.Contains(newWorkoutName))
            {
                int workoutCount = workoutNamesList.Count(n => n.StartsWith(newWorkoutName));
                newWorkoutName += workoutCount.ToString();
            }
            
            // Create a new workout instance copying the name, associating with the current user, and initializing an empty list for exercises
            var copiedWorkout = new Workout()
            {
                UserId = user.Id,
                Name = newWorkoutName,
                WorkoutHasExercises = new List<WorkoutHasExercise>(),
                ApplicationUser = user
            };
            
             // Create a copy of each exercise with the same ExerciseId, order, and set details.
            foreach (var exercise in originalWorkout.WorkoutHasExercises)
            {
                var copiedExercise = new WorkoutHasExercise()
                {
                    ExerciseId = exercise.ExerciseId,
                    IsOrder = exercise.IsOrder,
                    Sets = exercise.Sets
                };
                copiedWorkout.WorkoutHasExercises.Add(copiedExercise);
            }

            _db.Workouts.Add(copiedWorkout);
            await _db.SaveChangesAsync();

            // Create a new association between the copied routine and workout, preserving the day of the week
            var copiedRoutineHasWorkout = new RoutineHasWorkout()
            {
                RoutineId = copiedRoutine.Id,
                WorkoutId = copiedWorkout.Id,
                DayOfWeek = routineWorkout.DayOfWeek
            };

            copiedRoutine.RoutineHasWorkouts.Add(copiedRoutineHasWorkout);
        }
        
        _db.SharedRoutines.Remove(routine);
        await _db.SaveChangesAsync();
        
        
        return RedirectToAction(nameof(Manage));
    }
    
    // Post action to accept a shared routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeclineRoutine(int rid)
    {
        var user = await _um.GetUserAsync(User);
        
        // Retrieve the routine shared with the current user by the routine ID
        var pending = await _db.SharedRoutines
            .Where(sr => sr.RoutineId == rid && sr.ReceivingUserId == user.Id)
            .FirstOrDefaultAsync();

        if (pending == null) return NotFound();

        _db.SharedRoutines.Remove(pending);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    
    public async Task<IActionResult> RoutineContent(int rid)
    {
        var userId = _um.GetUserId(User);
        
        // Gets the routine with the provided id, the workouts associated with that routine and the exercises associated with those workouts
        var routineWithInfo = await _db.Routines
            .Where(r => r.Id == rid)
            .Include(r => r.RoutineHasWorkouts)
            .ThenInclude(rhw => rhw.Workout)
            .ThenInclude(w => w.WorkoutHasExercises)
            .ThenInclude(whe => whe.Exercise)
            .FirstOrDefaultAsync();
        
        // Checks if the user has access to the routine
        if (routineWithInfo.UserId != userId)
        {
            // Checks if the user has access to the routine through a shared routine
            var sharedRoutine = await _db.SharedRoutines
                .Where(sr => sr.RoutineId == rid && sr.ReceivingUserId == userId)
                .FirstOrDefaultAsync();
            
            if (sharedRoutine == null) return NotFound();
        }
        
        return View(routineWithInfo);
    } 
    
}