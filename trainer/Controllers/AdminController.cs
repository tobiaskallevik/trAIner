using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers;

[Authorize (Roles = "Admin, SuperAdmin", Policy = "NotBanned")]
public class AdminController : Controller
{
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    /*Injects the dependencies*/
    public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
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
}