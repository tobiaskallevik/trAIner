using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;



namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class WorkoutsController : Controller
{
    
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    public WorkoutsController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    // Creates a class to associate the exercise with the number of sets
    public class ExerciseSet
    {
        public int ExerciseId { get; set; }
        public string Name { get; set; }
        public int Sets { get; set; }
        public int IsOrder { get; set; }
        public string GifUrl { get; set; }
    }
    

    
    // Get action for the manage view
    public async Task<IActionResult> Manage()
    {
        // Gets the current user's id and the workouts associated with that user
        var user = await _um.GetUserAsync(User);
        var workouts = await _db.Workouts
            .Include(w => w.WorkoutHasExercises)
            .ThenInclude(we => we.Exercise)
            .Where(w => w.UserId == user.Id)
            .ToListAsync();
        
        return View(workouts);
    } 
    
    // Get action for the edit view
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _um.GetUserId(User);
        
        // Gets the exercises associated with the workout
        var exerciseSets = await _db.WorkoutHasExercises
            .Where(we => we.WorkoutId == id)
            .Join(_db.Exercises, we => we.ExerciseId, e => e.Id, 
                (we, e) => new { workoutHasExercise = we, Exercise = e })
            .Select(joined => new ExerciseSet 
            { 
                ExerciseId = joined.workoutHasExercise.ExerciseId, 
                Name = joined.Exercise.Name,
                Sets = joined.workoutHasExercise.Sets, 
                IsOrder = joined.workoutHasExercise.IsOrder, 
                GifUrl = joined.Exercise.GifUrl
            })
            .ToListAsync();
        
        // Gets the name and ID of the workout
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == id);
        ViewBag.Workout = new { Name = workout.Name, Id = workout.Id };
        
        // Stops the user from editing a workout that isn't theirs
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        return View(exerciseSets);
    }
    
    // Get action to show the exercises 
    public async Task<IActionResult> Exercises(int id)
    {
        // Gets the workout with the provided id
        var workout = await _db.Workouts.Include(w => w.WorkoutHasExercises).FirstOrDefaultAsync(w => w.Id == id);
        var userId = _um.GetUserId(User);

        // Stops the user from editing a workout that isn't theirs
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }

        // Creates a list for the exercises that are already associated with the workout
        var associatedExerciseIds = new List<int>();
        
        // Since a user could have created the workout without adding any exercises we need to check if the list is null
        if (workout.WorkoutHasExercises != null)
        {
            associatedExerciseIds = workout.WorkoutHasExercises.Select(e => e.ExerciseId).ToList();
        }

        // Gets the exercises that are not already associated with the workout
        var exercises = await _db.Exercises.Where(e => !associatedExerciseIds.Contains(e.Id)).ToListAsync();

        ViewBag.Workout = new { Name = workout.Name, Id = workout.Id };
        return View(exercises);
    }
    
    // Post action for adding exercises to a workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateWorkout(string Name)
    {
        // Gets the current user's id
        var user = await _um.GetUserAsync(User);
        
        // Creates a new workout with the users id and the provided workout name
        var workout = new Workout
        {
            Name = Name,
            UserId = user.Id,
            ApplicationUser = user
        };
        
        // Adds the workout to the DB
        _db.Workouts.Add(workout);
        await _db.SaveChangesAsync();
        
        // Returns the user to the manage view
        return RedirectToAction(nameof(Manage));
    }


    
    // Adds an exercise to a workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AddExerciseToWorkout(int workoutId, int exerciseId, int sets)
    {
        // Gets the id of the current user
        var userId = _um.GetUserId(User);
        // Gets the workout with the provided id
        var workout = _db.Workouts.FirstOrDefault(w => w.Id == workoutId);
        
        // Check if the current user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }

        // Creates a var to hold the order of the exercise
        var isOrder = 1;
        
        // If there are any exercises in the workout, the order of the new exercise will be the highest order + 1
        if (_db.WorkoutHasExercises.Any(we => we.WorkoutId == workoutId))
        {
            isOrder = _db.WorkoutHasExercises.Where(we => we.WorkoutId == workoutId).Max(we => we.IsOrder) + 1;
        }
        
        // Adds the exercise to the workout
        var workoutHasExercise = new WorkoutHasExercise
        {
            WorkoutId = workoutId,
            ExerciseId = exerciseId,
            Sets = sets,
            IsOrder = isOrder
        };
        
        _db.WorkoutHasExercises.Add(workoutHasExercise);
        _db.SaveChanges();
        
        return RedirectToAction("Edit", new { id = workoutId });
    }
    
    // Get action to show the users for choosing whom to share the workout with
    public async Task<IActionResult> GetAllFriends(int id)
    {
        // Fetches a specific workout by ID and lists all accepted friends of the current user for that workout
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == id);
        
        
        
        ViewBag.Workout = new { Name = workout.Name, Id = workout.Id };
        var userId = _um.GetUserId(User);
        
        // Checks if the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        var friends = await _db.Friends
            .Where(f => f.UserId == userId && f.Accepted)
            .Include(f => f.HasFriend)
            .ToListAsync();
        
        return View("Users", friends);
    }
    
    // Shares the workout with the chosen user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShareWorkout(int wid, string uid)
    {

        // Checks if there already is a pending workout for this workout and the selected user
        var wo = await _db.SharedWorkouts
            .Where(sw => sw.WorkoutId == wid && sw.ReceivingUserId == uid)
            .FirstOrDefaultAsync();
        // If the same workout is already shared, it redirects back to the manage page without creating a new share
        if (wo != null) return RedirectToAction(nameof(Manage));
        
        // Create a new share if none exists
        var sharedWorkout = new SharedWorkout
        {
            WorkoutId = wid,
            ReceivingUserId = uid
        };
        
        _db.SharedWorkouts.Add(sharedWorkout);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }

    
    // Deletes a workout along with the exercises associated with it in the WorkoutHasExercises table.
    // The deletion of the associated listings in the WorkoutHasRoutines table is handled automatically by the DB
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        // Find the workout with the provided id
        var workout = await _db.Workouts.FindAsync(id);
        var userId = _um.GetUserId(User);
        
        if (workout == null)
        {
            return NotFound();
        }
        
        // Check if the current user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
        
        // Remove the workout from the database
        _db.Workouts.Remove(workout);
        await _db.SaveChangesAsync();
        
        return RedirectToAction(nameof(Manage));
    }
    
    // Allows user to change the order of the exercises in the workout
    // Works by swapping the order of the exercise with the one above or below it
    // Connected by using AJAX in the jQuery file to update the database without reloading
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveUp(int exerciseId, int workoutId)
    {
        // Get the workoutHasExercise with the provided exerciseId and workoutId
        var workoutHasExercise = await _db.WorkoutHasExercises.FirstOrDefaultAsync(we => we.ExerciseId == exerciseId && we.WorkoutId == workoutId);
        var sameWorkoutHasExercises = await _db.WorkoutHasExercises.Where(we => we.WorkoutId == workoutHasExercise.WorkoutId).ToListAsync();
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutHasExercise.WorkoutId);
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        // Check if the exercise is already at the top
        if (workoutHasExercise.IsOrder == sameWorkoutHasExercises.Max(we => we.IsOrder))
        {
            return Ok();
        }

        // Find the exercise that is currently one position above
        var aboveExercise = sameWorkoutHasExercises.FirstOrDefault(we => we.IsOrder == workoutHasExercise.IsOrder + 1);

        // Swap positions
        workoutHasExercise.IsOrder++;
        aboveExercise.IsOrder--;

        await _db.SaveChangesAsync();
    
        return Ok();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MoveDown(int exerciseId, int workoutId)
    {
        // Get the workoutHasExercise with the provided exerciseId and workoutId
        var workoutHasExercise = await _db.WorkoutHasExercises.FirstOrDefaultAsync(we => we.ExerciseId == exerciseId && we.WorkoutId == workoutId);
        var sameWorkoutHasExercises = await _db.WorkoutHasExercises.Where(we => we.WorkoutId == workoutHasExercise.WorkoutId).ToListAsync();
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutHasExercise.WorkoutId);
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        // Check if the exercise is already at the bottom
        if (workoutHasExercise.IsOrder == sameWorkoutHasExercises.Min(we => we.IsOrder))
        {
            return Ok();
        }

        // Find the exercise that is currently one position below
        var belowExercise = sameWorkoutHasExercises.FirstOrDefault(we => we.IsOrder == workoutHasExercise.IsOrder - 1);

        // Swap positions
        workoutHasExercise.IsOrder--;
        belowExercise.IsOrder++;

        await _db.SaveChangesAsync();
    
        return Ok();
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetsUp(int exerciseId, int workoutId)
    {
        var userId = _um.GetUserId(User); 
        // Get the workoutHasExercise with the provided exerciseId and workoutId
        var workoutHasExercise = await _db.WorkoutHasExercises.FirstOrDefaultAsync(we => we.ExerciseId == exerciseId && we.WorkoutId == workoutId);
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutHasExercise.WorkoutId);

        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        // Ensures that the number of sets is not more than 10
        if (workoutHasExercise.Sets < 10)
        {
            workoutHasExercise.Sets++;
        }

        await _db.SaveChangesAsync();
    
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetsDown(int exerciseId, int workoutId)
    {
        var userId = _um.GetUserId(User); 
        // Get the workoutHasExercise with the provided exerciseId and workoutId
        var workoutHasExercise = await _db.WorkoutHasExercises.FirstOrDefaultAsync(we => we.ExerciseId == exerciseId && we.WorkoutId == workoutId);
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutHasExercise.WorkoutId);

        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        // Ensures that the number of sets is not less than 1
        if (workoutHasExercise.Sets > 1)
        {
            workoutHasExercise.Sets--;
        }

        await _db.SaveChangesAsync();
    
        return Ok();
    }

    
    // Deletes an exercise from the workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    
    public async Task<IActionResult> DeleteFromWorkout(int exerciseId, int workoutId)
    {
        // Get the workoutHasExercise with the provided exerciseId and workoutId
        var workoutHasExercise = await _db.WorkoutHasExercises.FirstOrDefaultAsync(we => we.ExerciseId == exerciseId && we.WorkoutId == workoutId);
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutHasExercise.WorkoutId);
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        _db.WorkoutHasExercises.Remove(workoutHasExercise);
        await _db.SaveChangesAsync();
    
        return Ok();
    }
    
    // Updates the name of the workout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateName(int workoutId, string name)
    {
        // Get the workout with the provided workoutId
        var workout = await _db.Workouts.FirstOrDefaultAsync(w => w.Id == workoutId);
        var userId = _um.GetUserId(User);
    
        // Ensures that the user is the owner of the workout
        if (workout.UserId != userId)
        {
            return RedirectToAction(nameof(Manage));
        }
    
        workout.Name = name;
        await _db.SaveChangesAsync();
    
        return Ok();
    }

}