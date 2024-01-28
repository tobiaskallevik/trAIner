using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;
using trainer.ViewModels;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class LogController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    public LogController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    // Gets the log menu view
    public async Task<IActionResult> YourLog()
    {
        var user = await _um.GetUserAsync(User);
        var logs = await _db.LoggedWorkouts.Where(w => w.UserId == user.Id).ToListAsync();
        
        var logViewModels = logs.Select(log => new LogViewModel
        {
            LoggedWorkout = log,
            WorkoutName = _db.Workouts.FirstOrDefault(w => w.Id == log.WorkoutId)?.Name
        }).ToList();
        
        return View(logViewModels);
    }
    
    public async Task<IActionResult> LogNew()
    {
        // Gets the current user's id and the workouts associated with that user
        var user = await _um.GetUserAsync(User);
    
        var workouts = await _db.Workouts
            .Include(w => w.WorkoutHasExercises)
            .ThenInclude(we => we.Exercise)
            .Where(w => w.UserId == user.Id)
            .ToListAsync();
        
        var viewModel = new RoutineWorkoutViewModel
        {
            Routines = _db.Routines.Where(r => r.UserId == user.Id).ToList(),
            Workouts = workouts,
            RoutineHasWorkouts = _db.RoutineHasWorkouts.Where(rhw => rhw.Workout.UserId == user.Id).ToList()
        };
        
        return View(viewModel);
    }

    public async Task<IActionResult> LoggedContent(int id)
    {
        // Gets the logged workout with the provided id and the current user's id
        var loggedWorkout = await _db.LoggedWorkouts.FirstOrDefaultAsync(lw => lw.Id == id);
        var userId = _um.GetUserId(User);
        
        // Ensures that the user is the owner of the workout
        if (loggedWorkout.UserId != userId)
        {
            return RedirectToAction(nameof(LogNew));
        }

        // Gets the name of the workout and the exercises associated with the logged workout
        var workoutName = _db.Workouts.Where(w => w.Id == loggedWorkout.WorkoutId).Select(w => w.Name).FirstOrDefault();
        var loggedWorkoutHasExercises = await _db.LoggedWorkoutHasExercises
            .Include(lwhe => lwhe.Exercise)  
            .Where(lwhe => lwhe.LoggedWorkoutId == id)
            .ToListAsync();
        var exercises = loggedWorkoutHasExercises.Select(lwhe => lwhe.Exercise);
        
        // Gets the last logged workout with. Aka the last time the user did this workout. It will be the second workout when ordered by date
        var lastLoggedWorkout = await _db.LoggedWorkouts
            .Where(lw => lw.WorkoutId == loggedWorkout.WorkoutId && lw.UserId == userId)
            .OrderByDescending(lw => lw.Date)
            .Skip(1)
            .FirstOrDefaultAsync();

        List<LoggedWorkoutHasExercise> lastLoggedWorkoutHasExercises = new List<LoggedWorkoutHasExercise>();

        // If lastLoggedWorkout is not null, get the exercises associated with it
        if (lastLoggedWorkout != null)
        {
            lastLoggedWorkoutHasExercises = await _db.LoggedWorkoutHasExercises
                .Include(lwhe => lwhe.Exercise)
                .Where(lwhe => lwhe.LoggedWorkoutId == lastLoggedWorkout.Id)
                .ToListAsync();
        }

        // Prints out the id of the last logged exercise
        foreach (var lastLoggedWorkoutHasExercise in lastLoggedWorkoutHasExercises)
        {
            Console.WriteLine(lastLoggedWorkoutHasExercise.Id);
        }

        // Creates a new log content view model
        var viewModel = new LoggedWorkoutViewModel
        {
            LoggedWorkout = loggedWorkout,
            WorkoutName = workoutName,
            Exercises = exercises,
            LoggedWorkoutHasExercises = loggedWorkoutHasExercises,
            LastLoggedWorkoutHasExercises = lastLoggedWorkoutHasExercises
        };
        
        return View(viewModel);
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteLoggedWorkout(int logId)
    {
        // Get the logged workout with the provided id
        var loggedWorkout = await _db.LoggedWorkouts.FirstOrDefaultAsync(lw => lw.Id == logId);
       
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (loggedWorkout.UserId != userId)
        {
            return RedirectToAction(nameof(YourLog));
        }
        
        _db.LoggedWorkouts.Remove(loggedWorkout);
        await _db.SaveChangesAsync();
    
        return Ok();
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteOnExit(int logId)
    {
        // Get the logged workout with the provided id
        var loggedWorkout = await _db.LoggedWorkouts.FirstOrDefaultAsync(lw => lw.Id == logId);
       
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (loggedWorkout.UserId != userId)
        {
            return RedirectToAction(nameof(YourLog));
        }
        
        _db.LoggedWorkouts.Remove(loggedWorkout);
        await _db.SaveChangesAsync();

        return NoContent();
    }
    
    // Adds a workout to the log and calls the logContent view to fill in the exercise info for that logged workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToLog(int id)
    {
        // Gets the current user's id and the workouts associated with that user
        var user = await _um.GetUserAsync(User);
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == id);
        
        // Ensures that the user is the owner of the workout
        if (workout.UserId != user.Id)
        {
            return RedirectToAction(nameof(LogNew));
        }
        
        // Creates a new logged workout
        var loggedWorkout = new LoggedWorkout
        {
            WorkoutId = workout.Id,
            UserId = user.Id,
            Date = DateTime.Now,
            StartTime = TimeOnly.FromDateTime(DateTime.Now),
        };
        
        _db.LoggedWorkouts.AddAsync(loggedWorkout);
        await _db.SaveChangesAsync();
        
        // Gets the exercises associated with the workout
        var workoutHasExercises = await _db.WorkoutHasExercises.Where(whe => whe.WorkoutId == workout.Id).ToListAsync();
        
        // Creates a new logged workout has exercise for each exercise associated with the workout
        foreach (var workoutHasExercise in workoutHasExercises)
        {
            var loggedWorkoutHasExercise = new LoggedWorkoutHasExercise
            {
                LoggedWorkoutId = loggedWorkout.Id,
                ExerciseId = workoutHasExercise.ExerciseId,
                IsOrder = workoutHasExercise.IsOrder,
                Sets = workoutHasExercise.Sets,
            };
            
            await _db.LoggedWorkoutHasExercises.AddAsync(loggedWorkoutHasExercise);
        }
        
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(LoggedContent), new { id = loggedWorkout.Id });
    }
    

    /*Adds info to logged workout*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddInfoToLog([FromForm] Microsoft.AspNetCore.Http.IFormCollection formData)
    {
        // Get the logged workout and user id
        var loggedWorkoutId = int.Parse(formData["LoggedWorkout.Id"]);
        var loggedWorkout = await _db.LoggedWorkouts.FindAsync(loggedWorkoutId);
        var userId = _um.GetUserId(User);

        if (loggedWorkout.UserId != userId)
        {
            return RedirectToAction(nameof(YourLog));
        }

        // Update each LoggedWorkoutHasExercise with the provided information
        foreach (var key in formData.Keys)
        {
            if (key.StartsWith("LoggedWorkoutHasExercises["))
            {
                var idString = key.Substring("LoggedWorkoutHasExercises[".Length, key.IndexOf(']') - "LoggedWorkoutHasExercises[".Length);
                var id = int.Parse(idString);
                var dbLoggedWorkoutHasExercise = await _db.LoggedWorkoutHasExercises.FindAsync(id);
                if (dbLoggedWorkoutHasExercise != null)
                {
                    if (key.EndsWith("].Reps"))
                    {
                        dbLoggedWorkoutHasExercise.Reps = int.Parse(formData[key]);
                    }
                    else if (key.EndsWith("].Weight"))
                    {
                        dbLoggedWorkoutHasExercise.Weight = int.Parse(formData[key]);
                    }
                    else if (key.EndsWith("].Min"))
                    {
                        dbLoggedWorkoutHasExercise.Min = int.Parse(formData[key]);
                    }
                }
            }
        }



        // Sets the end time for the workout
        if (loggedWorkout.EndTime == TimeOnly.Parse("00:00:00"))
        {
            loggedWorkout.EndTime = TimeOnly.FromDateTime(DateTime.Now);
        }

        // Save changes to the database
        await _db.SaveChangesAsync();

        return RedirectToAction(nameof(YourLog));
        /*Word of note: This is probably (definitely) not the best way to do it, but I had some wierd issues trying to make it update correctly using other methods. */
    }
}