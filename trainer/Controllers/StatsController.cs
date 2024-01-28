using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers;

[Authorize (Roles = "Admin", Policy = "NotBanned")]
public class StatsController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    /*Injects the dependencies*/
    public StatsController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    // GET
    public IActionResult Index()
    {
        return View();
    }
    
    // GET - account count
    [HttpGet]
    public async Task<IActionResult> GetUserCount()
    {
        int count = await _um.Users.CountAsync();
        
        return Ok(count);
    }
    
    // GET - workout count
    [HttpGet]
    public async Task<IActionResult> GetWorkoutCount()
    {
        int count = await _db.Workouts.CountAsync();
        
        return Ok(count);
    }
    
    // GET - exercise count
    [HttpGet]
    public async Task<IActionResult> GetExerciseCount()
    {
        int count = await _db.Exercises.CountAsync();
        
        return Ok(count);
    }
    
    // GET - Routine count
    [HttpGet]
    public async Task<IActionResult> GetRoutineCount()
    {
        int count = await _db.Routines.CountAsync();
        
        return Ok(count);
    }
    
    // GET - LoggedWorkouts count
    [HttpGet]
    public async Task<IActionResult> GetLoggedWorkoutCount()
    {
        int count = await _db.LoggedWorkouts.CountAsync();
        
        return Ok(count);
    }
}