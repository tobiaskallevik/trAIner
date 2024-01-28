using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class RoutinesController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    public RoutinesController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    public class RoutineSet
    {
        public int RWId { get; set; }
        public int WorkoutId { get; set; }
        public string WorkoutName { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
    }

    
    // Get action for the manage view
    // Shows the user all of their routines
    public async Task<IActionResult> Manage()
    {
        // Gets the current user's id and the routines associated with that user
        var user = await _um.GetUserAsync(User);
        var routines = await _db.Routines.Where(r => r.UserId == user.Id).ToListAsync();
        
        return View(routines);
    } 

    // Get action for the edit view
    // Shows the user the workouts associated with the routine they want to edit
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _um.GetUserId(User);
        
        // Gets the workouts associated with the routine
        var routineSets = await _db.RoutineHasWorkouts
            .Where(rw => rw.RoutineId == id)
            .Join(_db.Workouts, rw => rw.WorkoutId, w => w.Id, (rw, w) => new { RoutineHasWorkout = rw, Workout = w })
            .Select(joined => new RoutineSet 
            { 
                RWId = joined.RoutineHasWorkout.Id,
                WorkoutId = joined.RoutineHasWorkout.WorkoutId,
                WorkoutName = joined.Workout.Name,
                DayOfWeek = joined.RoutineHasWorkout.DayOfWeek
            })
            .ToListAsync();
        
        // Gets the name and ID of the routine
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == id);
        ViewBag.Routine = new { Name = routine.Name, Id = routine.Id };
        
        // Stops the user from editing a routine that isn't theirs
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        return View(routineSets);
    }
    
    
    // Get action to show the workouts 
    public async Task<IActionResult> AddWorkout(int id, int day)
    {
        // Gets the routine with the provided id
        var routine = await _db.Routines.Include(r => r.RoutineHasWorkouts).FirstOrDefaultAsync(r => r.Id == id);
        var userId = _um.GetUserId(User);

        // Stops the user from editing a routine that isn't theirs
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Edit));
        } 
        
        // Gets a list of the users workouts
        var workouts = await _db.Workouts
            .Include(w => w.WorkoutHasExercises)
            .ThenInclude(we => we.Exercise)  
            .Where(w => w.UserId == userId)
            .ToListAsync();
        
        ViewBag.Routine = new { Name = routine.Name, Id = routine.Id, Day = day };
        return View(workouts);
    }
    
    // Shows the user the templates they can use
    public async Task<IActionResult> RoutineTemplates()
    {
        var routines = await _db.Routines.Where(r => r.UserId == null).ToListAsync();
        
        return View(routines);
    } 
    
    // Shows the user the content of the template they want to use
    public async Task<IActionResult> RoutineContent(int id, string view)
    {
        
        // Gets the id of the current user and the routine with the provided id
        var userId = _um.GetUserId(User);
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == id);
 
        // Verifies that the user is the owner of the routine or that the routine is a template
        if (routine.UserId != userId && routine.UserId != null)
        {
            return RedirectToAction(nameof(RoutineTemplates));
        }
        
        // Gets the routine with the provided id, the workouts associated with that routine and the exercises associated with those workouts
        var routineWithInfo = await _db.Routines
            .Where(r => r.Id == id)
            .Include(r => r.RoutineHasWorkouts)
            .ThenInclude(rhw => rhw.Workout)
            .ThenInclude(w => w.WorkoutHasExercises)
            .ThenInclude(whe => whe.Exercise)
            .FirstOrDefaultAsync();
        
        // Viewbag that holds the info of the view that the user came from
        ViewBag.View = view;
        return View(routineWithInfo);
    } 
    
    
    // Post action for creating a new routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateRoutine(string Name)
    {
        // Gets the current user's id
        var user = await _um.GetUserAsync(User);
        
        // Creates a new workout with the users id and the provided workout name
        var routine = new Routine
        {
            Name = Name,
            UserId = user.Id,
            ApplicationUser = user
        };
        
        // Adds the workout to the DB
        _db.Routines.Add(routine);
        await _db.SaveChangesAsync();
        
        // Returns the user to the manage view
        return RedirectToAction(nameof(Manage));
    }
    
    // Get action to show the users for choosing whom to share the routine with
    public async Task<IActionResult> GetAllFriends(int id)
    {
        // Fetches a specific routine by ID and lists all accepted friends of the current user for that routine
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == id);
        ViewBag.Routine = new { Name = routine.Name, Id = routine.Id };
        var userId = _um.GetUserId(User);
        
        // Stops the user from accessing a routine that isn't theirs
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        var friends = await _db.Friends
            .Where(f => f.UserId == userId && f.Accepted)
            .Include(f => f.HasFriend)
            .ToListAsync();
        return View("Users", friends);
    }
    
    // Shares the routine with the chosen user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShareRoutine(int rid, string uid)
    {
        // Checks if there already is a pending routine for this routine and the selected user
        var r = await _db.SharedRoutines
            .Where(sr => sr.RoutineId == rid && sr.ReceivingUserId == uid)
            .FirstOrDefaultAsync();
        // If the same routine is already shared, it redirects back to the manage page without creating a new share
        if (r != null) return RedirectToAction(nameof(Manage));
        
        // Create a new share if none exists
        var sharedRoutine = new SharedRoutine
        {
            RoutineId = rid,
            ReceivingUserId = uid
        };

        _db.SharedRoutines.Add(sharedRoutine);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    
    // Deletes a routine along with the exercises associated with it in the RoutineHasExercises table
    // The deletion of the associated listings in the RoutineHasWorkouts table is handled automatically by the DB
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Find the workout with the provided id
        var routine = await _db.Routines.FindAsync(id);
        var userId = _um.GetUserId(User);
        
        if (routine == null)
        {
            return NotFound();
        }
        
        // Check if the current user is the owner of the routine
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        // Remove the routine from the database
        _db.Routines.Remove(routine);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    
    // Post action for deleting a workout from the routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFromRoutine(int rWId, int workoutId, int routineId)
    {
        // Get the RoutineHasWorkout with the provided routineId and workoutId
        var routineHasWorkout = await _db.RoutineHasWorkouts.FirstOrDefaultAsync(rw => rw.WorkoutId == workoutId 
            && rw.RoutineId == routineId && rw.Id == rWId);
        
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == routineHasWorkout.RoutineId);
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        _db.RoutineHasWorkouts.Remove(routineHasWorkout);
        await _db.SaveChangesAsync();
    
        return Ok();
    }
    
    // Allows user to change the order of the workout in the routine
    // Works by swapping the day with the one above or below it the is one. If not it just increases or decreases the day
    // Connected by using AJAX in the jQuery file to update the database without reloading
    // Done in an atomic transaction to ensure that the database is not left in an invalid state
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int rWId, int workoutId, int routineId)
    {
        using var transaction = _db.Database.BeginTransaction();
    
        try
        {
            // Get the routineHasWorkout with the provided workoutId, routineId and day
            var routineHasWorkout = await _db.RoutineHasWorkouts.FirstOrDefaultAsync(rw => rw.WorkoutId == workoutId 
                && rw.RoutineId == routineId && rw.Id == rWId);
            
            var sameRoutineHasWorkout = await _db.RoutineHasWorkouts.Where(rw => rw.RoutineId == routineHasWorkout.RoutineId).ToListAsync();
            var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == routineHasWorkout.RoutineId);
            var userId = _um.GetUserId(User);
    
            // Ensures that the user is the owner of the workout
            if (routine.UserId != userId)
            {
                return RedirectToAction(nameof(Manage));
            }
    
            // Check if the workout is already at the top
            // DayOfWeek starts at 0 (Sunday) and ends at 6 (Saturday). 
            // This is the international standard
            if (routineHasWorkout.DayOfWeek == DayOfWeek.Sunday)
            {
                return Ok();
            }
            
            // Checks if a workout exist above the current one
            var aboveWorkout = sameRoutineHasWorkout.FirstOrDefault(rw => rw.DayOfWeek == routineHasWorkout.DayOfWeek - 1);
            
            if (aboveWorkout != null)
            {
                // Temporarily set aboveWorkout.DayOfWeek to an invalid value. Needed since DayOfWeek is a unique constraint
                aboveWorkout.DayOfWeek = (DayOfWeek)7;
                await _db.SaveChangesAsync();

                // Swap the days
                aboveWorkout.DayOfWeek = routineHasWorkout.DayOfWeek;
                routineHasWorkout.DayOfWeek--;
            }
            else
            {
                    routineHasWorkout.DayOfWeek--;
            }
            
            await _db.SaveChangesAsync();
    
            transaction.Commit();
        }
        catch (Exception)
        {
            // Rollback the transaction if an error occurs
            transaction.Rollback();
        }
    
        return Ok();
    }
    
    // Allows user to change the order of the workout in the routine
    // Works by swapping the day with the one above or below it the is one. If not it just increases or decreases the day
    // Connected by using AJAX in the jQuery file to update the database without reloading
    // Done in an atomic transaction to ensure that the database is not left in an invalid state
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int rWId, int workoutId, int routineId)
    {
        using var transaction = _db.Database.BeginTransaction();
    
        try
        {
            // Get the routineHasWorkout with the provided workoutId, routineId and day
            var routineHasWorkout = await _db.RoutineHasWorkouts.FirstOrDefaultAsync(rw => rw.WorkoutId == workoutId && rw.RoutineId == routineId && rw.Id == rWId);
            var sameRoutineHasWorkout = await _db.RoutineHasWorkouts.Where(rw => rw.RoutineId == routineHasWorkout.RoutineId).ToListAsync();
            var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == routineHasWorkout.RoutineId);
            var userId = _um.GetUserId(User);
    
            // Ensures that the user is the owner of the workout
            if (routine.UserId != userId)
            {
                return RedirectToAction(nameof(Manage));
            }
    
            // Check if the workout is already at the top
            // DayOfWeek starts at 0 (Sunday) and ends at 6 (Saturday). 
            // This is the international standard
            if (routineHasWorkout.DayOfWeek == DayOfWeek.Saturday)
            {
                return Ok();
            }
            
            // Checks if a workout exist above the current one
            var belowWorkout = sameRoutineHasWorkout.FirstOrDefault(rw => rw.DayOfWeek == routineHasWorkout.DayOfWeek + 1);
            
            if (belowWorkout != null)
            {
                // Temporarily set aboveWorkout.DayOfWeek to an invalid value. Needed since DayOfWeek is a unique constraint
                belowWorkout.DayOfWeek = (DayOfWeek)7;
                await _db.SaveChangesAsync();

                // Swap the days
                belowWorkout.DayOfWeek = routineHasWorkout.DayOfWeek;
                routineHasWorkout.DayOfWeek++;
                
            }
            else
            {
                routineHasWorkout.DayOfWeek++;
            }
            
            await _db.SaveChangesAsync();
    
            transaction.Commit();
        }
        catch (Exception)
        {
            // Rollback the transaction if an error occurs
            transaction.Rollback();
        }
    
        return Ok();
    }
    
    // Post action for adding a workout to the routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddWorkoutToRoutine(int routineId, int workoutId, int routineDay)
    {
        
        // Gets routine and the current user
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == routineId);
        var userId = _um.GetUserId(User);
        
        // Ensures that the user is the owner of the routine
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        // Creates a new RoutineHasWorkout with the provided routineId, workoutId and day
        var routineHasWorkout = new RoutineHasWorkout
        {
            RoutineId = routineId,
            WorkoutId = workoutId,
            DayOfWeek = (DayOfWeek)routineDay
        };
        
        // Adds it to the db
        _db.RoutineHasWorkouts.Add(routineHasWorkout);
        await _db.SaveChangesAsync();
        
        return RedirectToAction("Edit", new { id = routineId });
    }
    
    // Post action for updating the name of a routine
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateName(int routineId, string name)
    {
        // Gets the routine with the provided id
        Console.WriteLine("Now in the post action for updating name");
        var routine = await _db.Routines.FirstOrDefaultAsync(r => r.Id == routineId);
        var userId = _um.GetUserId(User);
        
        // Ensures that the user is the owner of the routine
        if (routine.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        // Updates the name of the routine
        routine.Name = name;
        await _db.SaveChangesAsync();
        
        return Ok();
    }
    
    //Post action for adding a routine from a template
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddTemplate(int routineId)
    {
        // Adds the routine to the shared routines table
        
        // Gets the current user's id
        var userId = _um.GetUserId(User);
        
        // Checks if there already is a pending routine for this routine and the selected user
        var r = await _db.SharedRoutines
            .Where(sr => sr.RoutineId == routineId && sr.ReceivingUserId == userId)
            .FirstOrDefaultAsync();
        
        // Create a new share if none exists
        var sharedRoutine = new SharedRoutine
        {
            RoutineId = routineId,
            ReceivingUserId = userId
        };
        
        _db.SharedRoutines.Add(sharedRoutine);
        await _db.SaveChangesAsync();
        
        // Retrieves the currently logged in user and the specific shared routine with the workouts and exercises
        var user = await _um.GetUserAsync(User);
        
        var routine = await _db.SharedRoutines
            .Where(sr => sr.RoutineId == routineId && sr.ReceivingUserId == user.Id)
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
}

