namespace trainer.Data;
using Microsoft.AspNetCore.Identity;
using trainer.Models;
using Newtonsoft.Json;
using System.IO;

public class ApplicationDbInitializer
{
    public static void Initialize(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        // Delete and create database on each run
        // This method might need to change as the DB grows
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        
        // Adds test users
        var user1 = new ApplicationUser { UserName = "user1@gmail.com", Nickname = "user1", Email = "user1@gmail.com", EmailConfirmed = true};
        var user2 = new ApplicationUser { UserName = "user2@gmail.com", Nickname = "user2", Email = "user2@gmail.com", EmailConfirmed = true};
        var user3 = new ApplicationUser { UserName = "user3@gmail.com", Nickname = "user3", Email = "user3@gmail.com", EmailConfirmed = true};
        var user4 = new ApplicationUser { UserName = "user4@gmail.com", Nickname = "user4", Email = "user4@gmail.com", EmailConfirmed = true};
        var user5 = new ApplicationUser { UserName = "user5@gmail.com", Nickname = "user5", Email = "user5@gmail.com", EmailConfirmed = true};
        var user6 = new ApplicationUser { UserName = "user6@gmail.com", Nickname = "user6", Email = "user6@gmail.com", EmailConfirmed = true};
        var user7 = new ApplicationUser { UserName = "user7@gmail.com", Nickname = "user7", Email = "user7@gmail.com", EmailConfirmed = true};
        
        um.CreateAsync(user1, "Password1.").Wait();
        um.CreateAsync(user2, "Password1.").Wait();
        um.CreateAsync(user3, "Password1.").Wait();
        um.CreateAsync(user4, "Password1.").Wait();
        um.CreateAsync(user5, "Password1.").Wait();
        um.CreateAsync(user6, "Password1.").Wait();
        um.CreateAsync(user7, "Password1.").Wait();
        
        // Adds Admin
        rm.CreateAsync(new IdentityRole("SuperAdmin")).Wait();
        rm.CreateAsync(new IdentityRole("Admin")).Wait();
        rm.CreateAsync(new IdentityRole("Banned")).Wait();
        um.AddToRoleAsync(user1, "SuperAdmin").Wait();
        um.AddToRoleAsync(user1, "Admin").Wait();
        um.AddToRoleAsync(user2, "Admin").Wait();
        um.AddToRoleAsync(user3, "Banned").Wait();
        db.SaveChanges();
        
        // Adds test friends
        var testFriends = new List<Friend>
        {
            new Friend { UserId = user1.Id, HasFriendId = user2.Id, Accepted = true},
            new Friend { UserId = user2.Id, HasFriendId = user1.Id, Accepted = true},
            new Friend { UserId = user4.Id, HasFriendId = user1.Id, Accepted = false},
        };
        
        foreach (var friend in testFriends)
        {
            db.Friends.Add(friend);
        }
        
        db.SaveChanges();
        
        // Adds test workouts and templates. Those without a user are templates
        var testWorkouts = new List<Workout>
        {
            new Workout { Id = 1, Name = "Chest Day", UserId = user1.Id, ApplicationUser = user1 },
            new Workout { Id = 2, Name = "Leg Day", UserId = user1.Id, ApplicationUser = user1 },
            new Workout { Id = 3, Name = "Upper-Lower", UserId = user1.Id, ApplicationUser = user1 },
            new Workout { Id = 4, Name = "Back Day", UserId = user1.Id, ApplicationUser = user1 },
            new Workout { Id = 5, Name = "Arm Day", UserId = user1.Id, ApplicationUser = user1},
            new Workout { Id = 6, Name = "Push Day Template"},
            new Workout { Id = 7, Name = "Pull Day Template"},
            new Workout { Id = 8, Name = "Leg Day Template"},
            new Workout { Id = 9, Name = "Upper Day Template"},
            new Workout { Id = 10, Name = "Arm Day Template"},
            new Workout { Id = 11, Name = "Chest Day Template"},
            new Workout { Id = 12, Name = "Back Day Template"},
            new Workout { Id = 13, Name = "Shoulder Day Template"}
        };

        foreach (var workout in testWorkouts)
        {
            db.Workouts.Add(workout);
        }
        
        db.SaveChanges();
        
        // Parses the json file and adds the exercises to the database
        string json = File.ReadAllText("Data/exerciseInfo.json");
        var exercises = JsonConvert.DeserializeObject<List<Exercise>>(json);
        
        foreach (var exercise in exercises)
        {
            db.Exercises.Add(exercise);
        }

        db.SaveChanges();

        // Adds test exercises to test workouts
        var testWorkoutHasExercise = new List<WorkoutHasExercise>
        {
            new WorkoutHasExercise {WorkoutId = 1, ExerciseId = 35,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 1, ExerciseId = 262,  Sets = 2, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 1, ExerciseId = 3,  Sets = 4, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 1, ExerciseId = 224,  Sets = 5, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 1, ExerciseId = 279,  Sets = 1, IsOrder = 5},
            new WorkoutHasExercise {WorkoutId = 2, ExerciseId = 60,  Sets = 1, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 2, ExerciseId = 202,  Sets = 5, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 2, ExerciseId = 471,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 2, ExerciseId = 470,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 4,  Sets = 5, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 35,  Sets = 6, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 202,  Sets = 2, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 3,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 262,  Sets = 3, IsOrder = 5},
            new WorkoutHasExercise {WorkoutId = 3, ExerciseId = 224,  Sets = 3, IsOrder = 6},
            new WorkoutHasExercise {WorkoutId = 4, ExerciseId = 1,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 4, ExerciseId = 224,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 4, ExerciseId = 279,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 4, ExerciseId = 259,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 6, ExerciseId = 35,  Sets = 4, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 6, ExerciseId = 262,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 6, ExerciseId = 279,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 6, ExerciseId = 224,  Sets = 5, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 7, ExerciseId = 243,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 7, ExerciseId = 259,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 7, ExerciseId = 263,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 7, ExerciseId = 257,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 8, ExerciseId = 60,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 8, ExerciseId = 202,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 8, ExerciseId = 471,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 8, ExerciseId = 470,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 35,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 262,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 243,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 259,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 257,  Sets = 3, IsOrder = 5},
            new WorkoutHasExercise {WorkoutId = 9, ExerciseId = 224,  Sets = 3, IsOrder = 6},
            new WorkoutHasExercise {WorkoutId = 10, ExerciseId = 35,  Sets = 4, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 10, ExerciseId = 262,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 10, ExerciseId = 279,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 10, ExerciseId = 23,  Sets = 6, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 11, ExerciseId = 60,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 11, ExerciseId = 202,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 11, ExerciseId = 471,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 11, ExerciseId = 390,  Sets = 3, IsOrder = 4},
            new WorkoutHasExercise {WorkoutId = 12, ExerciseId = 47,  Sets = 3, IsOrder = 1},
            new WorkoutHasExercise {WorkoutId = 12, ExerciseId = 77,  Sets = 3, IsOrder = 2},
            new WorkoutHasExercise {WorkoutId = 12, ExerciseId = 111,  Sets = 3, IsOrder = 3},
            new WorkoutHasExercise {WorkoutId = 12, ExerciseId = 115,  Sets = 3, IsOrder = 4},
            
     
        };
        
        foreach (var workoutHasExercise in testWorkoutHasExercise)
        {
            db.WorkoutHasExercises.Add(workoutHasExercise);
        }
        
        db.SaveChanges();
        
        // Adds test routines
        var testRoutine = new List<Routine>
        {
            new Routine { Id = 1, Name = "Bro Split", UserId = user1.Id, ApplicationUser = user1 },
            new Routine { Id = 2, Name = "Upper Lower", UserId = user1.Id, ApplicationUser = user1 },
            
            new Routine { Id = 3, Name = "PPL Template"},
            new Routine { Id = 4, Name = "UpLow Template" },
            new Routine { Id = 5, Name = "Bro Split Template" },
        };

        foreach (var routine in testRoutine)
        {
            db.Routines.Add(routine);
        }
        
        db.SaveChanges();

        // Adds test routines to test workouts
        var testRoutineHasWorkout = new List<RoutineHasWorkout>
        {
            new RoutineHasWorkout { WorkoutId = 1, RoutineId = 1,  DayOfWeek = DayOfWeek.Monday},
            new RoutineHasWorkout { WorkoutId = 2, RoutineId = 1,  DayOfWeek = DayOfWeek.Tuesday},
            new RoutineHasWorkout { WorkoutId = 3, RoutineId = 1,  DayOfWeek = DayOfWeek.Thursday},
            new RoutineHasWorkout { WorkoutId = 4, RoutineId = 1,  DayOfWeek = DayOfWeek.Friday},
            
            new RoutineHasWorkout { WorkoutId = 1, RoutineId = 2,  DayOfWeek = DayOfWeek.Monday},
            new RoutineHasWorkout { WorkoutId = 2, RoutineId = 2,  DayOfWeek = DayOfWeek.Tuesday},
            new RoutineHasWorkout { WorkoutId = 1, RoutineId = 2,  DayOfWeek = DayOfWeek.Thursday},
            new RoutineHasWorkout { WorkoutId = 2, RoutineId = 2,  DayOfWeek = DayOfWeek.Friday},
            
            new RoutineHasWorkout { WorkoutId = 6, RoutineId = 3,  DayOfWeek = DayOfWeek.Monday},
            new RoutineHasWorkout { WorkoutId = 7, RoutineId = 3,  DayOfWeek = DayOfWeek.Wednesday},
            new RoutineHasWorkout { WorkoutId = 8, RoutineId = 3,  DayOfWeek = DayOfWeek.Friday},
            
            new RoutineHasWorkout { WorkoutId = 12, RoutineId = 4,  DayOfWeek = DayOfWeek.Monday},
            new RoutineHasWorkout { WorkoutId = 11, RoutineId = 4,  DayOfWeek = DayOfWeek.Tuesday},
            new RoutineHasWorkout { WorkoutId = 10, RoutineId = 4,  DayOfWeek = DayOfWeek.Friday},
            new RoutineHasWorkout { WorkoutId = 8, RoutineId = 4,  DayOfWeek = DayOfWeek.Saturday},
        };
   

        foreach (var routineHasWorkout in testRoutineHasWorkout)
        {
            db.RoutineHasWorkouts.Add(routineHasWorkout);
        }
        
        db.SaveChanges();
        
        // Adds test logged workouts
        var testLoggedWorkouts = new List<LoggedWorkout>
        {
            new LoggedWorkout { WorkoutId = 1, UserId = user1.Id, Date = DateTime.Now.AddDays(-13), StartTime = TimeOnly.Parse("08:00"), EndTime = TimeOnly.Parse("09:30") },
            new LoggedWorkout { WorkoutId = 1, UserId = user1.Id, Date = DateTime.Now.AddDays(-10), StartTime = TimeOnly.Parse("09:00"), EndTime = TimeOnly.Parse("10:15") },
            new LoggedWorkout { WorkoutId = 1, UserId = user1.Id, Date = DateTime.Now.AddDays(-8), StartTime = TimeOnly.Parse("07:30"), EndTime = TimeOnly.Parse("08:45") },
            new LoggedWorkout { WorkoutId = 1, UserId = user1.Id, Date = DateTime.Now.AddDays(-4), StartTime = TimeOnly.Parse("10:30"), EndTime = TimeOnly.Parse("11:45") },
            new LoggedWorkout { WorkoutId = 1, UserId = user1.Id, Date = DateTime.Now.AddDays(-2), StartTime = TimeOnly.Parse("11:00"), EndTime = TimeOnly.Parse("12:15") },
            new LoggedWorkout { WorkoutId = 2, UserId = user1.Id, Date = DateTime.Now.AddDays(-13), StartTime = TimeOnly.Parse("08:00"), EndTime = TimeOnly.Parse("09:30") },
            new LoggedWorkout { WorkoutId = 2, UserId = user1.Id, Date = DateTime.Now.AddDays(-10), StartTime = TimeOnly.Parse("09:00"), EndTime = TimeOnly.Parse("10:15") },
            new LoggedWorkout { WorkoutId = 2, UserId = user1.Id, Date = DateTime.Now.AddDays(-8), StartTime = TimeOnly.Parse("07:30"), EndTime = TimeOnly.Parse("08:45") },
            new LoggedWorkout { WorkoutId = 2, UserId = user1.Id, Date = DateTime.Now.AddDays(-4), StartTime = TimeOnly.Parse("10:30"), EndTime = TimeOnly.Parse("11:45") },
            new LoggedWorkout { WorkoutId = 2, UserId = user1.Id, Date = DateTime.Now.AddDays(-2), StartTime = TimeOnly.Parse("11:00"), EndTime = TimeOnly.Parse("12:15") }
        };

        foreach (var loggedWorkout in testLoggedWorkouts)
        {
            db.LoggedWorkouts.Add(loggedWorkout);
        }

        db.SaveChanges();

        // Adds test logged workouts exercises
        var testLoggedWorkoutHasExercises = new List<LoggedWorkoutHasExercise>
        {
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[0].Id, ExerciseId = 35, Sets = 3, Weight = 100, Reps = 10 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[0].Id, ExerciseId = 3, Sets = 4, Weight = 80, Reps = 8 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[1].Id, ExerciseId = 35, Sets = 3, Weight = 120, Reps = 6 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[1].Id, ExerciseId = 3, Sets = 4, Weight = 70, Reps = 12 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[2].Id, ExerciseId = 35, Sets = 3, Weight = 90, Reps = 10 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[2].Id, ExerciseId = 3, Sets = 4, Weight = 110, Reps = 8 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[3].Id, ExerciseId = 35, Sets = 3, Weight = 95, Reps = 9 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[3].Id, ExerciseId = 3, Sets = 4, Weight = 105, Reps = 7 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[4].Id, ExerciseId = 35, Sets = 3, Weight = 85, Reps = 11 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[4].Id, ExerciseId = 3, Sets = 4, Weight = 140, Reps = 10 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[5].Id, ExerciseId = 60, Sets = 3, Weight = 100, Reps = 10 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[5].Id, ExerciseId = 202, Sets = 5, Weight = 80, Reps = 8 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[6].Id, ExerciseId = 60, Sets = 3, Weight = 120, Reps = 6 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[6].Id, ExerciseId = 202, Sets = 5, Weight = 70, Reps = 12 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[7].Id, ExerciseId = 60, Sets = 3, Weight = 90, Reps = 10 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[7].Id, ExerciseId = 202, Sets = 5, Weight = 110, Reps = 8 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[8].Id, ExerciseId = 60, Sets = 3, Weight = 95, Reps = 9 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[8].Id, ExerciseId = 202, Sets = 5, Weight = 105, Reps = 7 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[9].Id, ExerciseId = 60, Sets = 3, Weight = 85, Reps = 11 },
            new LoggedWorkoutHasExercise { LoggedWorkoutId = testLoggedWorkouts[9].Id, ExerciseId = 202, Sets = 5, Weight = 140, Reps = 10 }
        };

        foreach (var loggedWorkoutHasExercise in testLoggedWorkoutHasExercises)
        {
            db.LoggedWorkoutHasExercises.Add(loggedWorkoutHasExercise);
        }

        db.SaveChanges();
    }
}