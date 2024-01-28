using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using trainer.Models;

namespace trainer.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    // Creates tables in the database 
    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Workout> Workouts { get; set; }
    public DbSet<Exercise> Exercises { get; set; }
    public DbSet<Routine> Routines { get; set; }
    public DbSet<WorkoutHasExercise> WorkoutHasExercises { get; set; }
    public DbSet<RoutineHasWorkout> RoutineHasWorkouts { get; set; }
    public DbSet<LoggedWorkout> LoggedWorkouts { get; set; }
    public DbSet<LoggedWorkoutHasExercise> LoggedWorkoutHasExercises { get; set; }
    public DbSet<Friend> Friends { get; set; }
    public DbSet<SharedWorkout> SharedWorkouts { get; set; }
    public DbSet<SharedRoutine> SharedRoutines { get; set; }


    // Creates the many-to-many relationships 
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Creates the relationship between users and workouts
        modelBuilder.Entity<Workout>()
            .HasOne(w => w.ApplicationUser)
            .WithMany(a => a.Workouts)
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Creates the relationship between users and routines
        modelBuilder.Entity<Routine>()
            .HasOne(r => r.ApplicationUser)
            .WithMany(a => a.Routines)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Creates the relationship between users and logged workouts
        modelBuilder.Entity<LoggedWorkout>()
            .HasOne(lw => lw.ApplicationUser)
            .WithMany(a => a.LoggedWorkouts)
            .HasForeignKey(lw => lw.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Many-to-many relationship between workouts and exercises
        // Creates a composite key
        modelBuilder.Entity<WorkoutHasExercise>()
            .HasKey(whe => new { whe.WorkoutId, whe.ExerciseId });

        // Creates the foreign keys
        modelBuilder.Entity<WorkoutHasExercise>()
            .HasOne(whe => whe.Workout)
            .WithMany(w => w.WorkoutHasExercises)
            .HasForeignKey(whe => whe.WorkoutId);

        modelBuilder.Entity<WorkoutHasExercise>()
            .HasOne(whe => whe.Exercise)
            .WithMany(e => e.WorkoutHasExercises)
            .HasForeignKey(whe => whe.ExerciseId);
        
        // Many-to-many relationship between routines and workouts
        // Creates the foreign keys
        modelBuilder.Entity<RoutineHasWorkout>()
            .HasOne(rhw => rhw.Workout)
            .WithMany(w => w.RoutineHasWorkouts) 
            .HasForeignKey(rhw => rhw.WorkoutId);

        modelBuilder.Entity<RoutineHasWorkout>()
            .HasOne(rhw => rhw.Routine)
            .WithMany(r => r.RoutineHasWorkouts) 
            .HasForeignKey(whr => whr.RoutineId);
        
        // Sets unique constraints (one workout per day, per routine)
        modelBuilder.Entity<RoutineHasWorkout>()
            .HasIndex(rhw => new { rhw.RoutineId, rhw.DayOfWeek })
            .IsUnique();

        // Many-to-many relationship between logged workouts and exercises
        // Creates the foreign keys
        modelBuilder.Entity<LoggedWorkoutHasExercise>()
            .HasOne(lwhe => lwhe.LoggedWorkout)
            .WithMany(w => w.LoggedWorkoutHasExercises)
            .HasForeignKey(lwhe => lwhe.LoggedWorkoutId);

        modelBuilder.Entity<LoggedWorkoutHasExercise>()
            .HasOne(lwhe => lwhe.Exercise)
            .WithMany(e => e.LoggedWorkoutHasExercises)
            .HasForeignKey(lwhe => lwhe.ExerciseId);
        
        // Many-to-many relationship between users
        // Creates a key
        modelBuilder.Entity<Friend>()
            .HasKey(f => new { f.UserId, f.HasFriendId });

        modelBuilder.Entity<Friend>()
            .HasOne(f => f.User)
            .WithMany(a => a.HasFriends)
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Friend>()
            .HasOne(f => f.HasFriend)
            .WithMany(a => a.User)
            .HasForeignKey(f => f.HasFriendId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Many-to-many relationship between users and SharedWorkouts
        // Creates a key
        modelBuilder.Entity<SharedWorkout>()
            .HasKey(s => new { s.ReceivingUserId, s.WorkoutId });

        modelBuilder.Entity<SharedWorkout>()
            .HasOne(s => s.PendingWorkout)
            .WithMany(w => w.ReceivingUsers)
            .HasForeignKey(s => s.WorkoutId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<SharedWorkout>()
            .HasOne(s => s.ReceivingUser)
            .WithMany(a => a.PendingWorkouts)
            .HasForeignKey(s => s.ReceivingUserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Many-to-many relationship between users and SharedRoutines
        // Creates a key
        modelBuilder.Entity<SharedRoutine>()
            .HasKey(s => new { s.ReceivingUserId, s.RoutineId });

        modelBuilder.Entity<SharedRoutine>()
            .HasOne(s => s.PendingRoutine)
            .WithMany(r => r.ReceivingUsers)
            .HasForeignKey(s => s.RoutineId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<SharedRoutine>()
            .HasOne(s => s.ReceivingUser)
            .WithMany(a => a.PendingRoutines)
            .HasForeignKey(s => s.ReceivingUserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    
}