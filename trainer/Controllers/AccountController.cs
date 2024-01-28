using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.V4.Pages.Account.Internal;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;
using System.Diagnostics;

namespace trainer.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        throw new NotImplementedException();
    }

    public IActionResult Register()
    {
        throw new NotImplementedException();
    }
    
    private ApplicationDbContext _db;
    private UserManager<ApplicationUser> _um;
    private RoleManager<IdentityRole> _rm;

    /*Injects the dependencies*/
    public AccountController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm)
    {
        _db = db;
        _um = um;
        _rm = rm;
    }
    
    // GET: Users
    [Authorize(Roles = "Admin, SuperAdmin", Policy = "NotBanned")]
    public async Task<IActionResult> Index()
    {
        if (_db.ApplicationUsers == null)
        {
            return Problem("Entity set 'ApplicationDbContext.ApplicationUsers'  is null.");
        }
        
        var users = await _db.ApplicationUsers.ToListAsync();
        var userViewModels = new List<UserViewModel>();
        
        foreach (var user in users)
        {
            var userViewModel = new UserViewModel
            {
                User = user,
                IsAdmin = await _um.IsInRoleAsync(user, "Admin"),
                IsSuperAdmin = await _um.IsInRoleAsync(user, "SuperAdmin"),
                IsBanned = await _um.IsInRoleAsync(user, "Banned")
            };
            userViewModels.Add(userViewModel);
        }

        return View(userViewModels);
    }
    
    // Assign/unassign either Admin or Banned role to user
    [HttpPost]
    [Authorize(Roles = "Admin, SuperAdmin", Policy = "NotBanned")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateUserRole(string userId, string action)
    {
        // Get user
        var user = await _um.FindByIdAsync(userId);
        if (user == null)
        {
            return Json( new { success = false, message = userId + " not found" });
        }
        
        // Gets the role of the user who is trying to change the role of another user
        var currentUser = await _um.GetUserAsync(User);
        var currentUserRole = await _um.GetRolesAsync(currentUser);

        // Prevents users from changing their own role
        if (userId == currentUser.Id)
        {
            return Json( new { success = false, message = "You cannot assign roles to yourself" });
        }
        
        // Prevents users from changing the role of a SuperAdmin
        if (await _um.IsInRoleAsync(user, "SuperAdmin"))
        {
            return Json( new { success = false, message = "You cannot assign or unassign roles to a Super Admin" });
        }
        
        // Prevents admins to ban other admins
        if (currentUserRole.Contains("Admin") && !currentUserRole.Contains("SuperAdmin") && await _um.IsInRoleAsync(user, "Admin") && (action == "Ban" || action == "Unban"))
        {
            return Json( new { success = false, message = "You cannot ban or unban an Admin" });
        }

        IdentityResult result = null;

        switch (action)
        {
            case "MakeAdmin":
                //Only super admins can make admins
                if (currentUserRole.Contains("SuperAdmin"))
                {
                    // Only make user admin if they are not already admin
                    if (!await _um.IsInRoleAsync(user, "Admin"))
                    {
                        result = await _um.AddToRoleAsync(user, "Admin");
                    }
                    else
                    {
                        return Json(new { success = false, message = "User is already admin" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "User is not Super Admin" });
                }
                break;
            case "RemoveAdmin":
                //Only super admins can remove admins
                if (currentUserRole.Contains("SuperAdmin"))
                {
                    // Only remove user from admin if they are admin
                    if (await _um.IsInRoleAsync(user, "Admin"))
                    {
                        result = await _um.RemoveFromRoleAsync(user, "Admin");
                    }
                    else
                    {
                        return Json(new { success = false, message = "User is not admin" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "You are not Super Admin, and cannot make assign admin to anyone" });
                }
                break;
            
            case "Ban":
                // Only ban user if they are not already banned
                if (!await _um.IsInRoleAsync(user, "Banned"))
                {
                    result = await _um.AddToRoleAsync(user, "Banned");
                    
                    if (result.Succeeded)
                    {
                        // Update the user´s security stamp to force a logout
                        await _um.UpdateSecurityStampAsync(user);
                    }
                }
                else
                {
                    return Json(new { success = false, message = "User is already banned" });
                }
                break;
            
            case "Unban":
                // Only unban user if they are banned
                if (await _um.IsInRoleAsync(user, "Banned"))
                {
                    result = await _um.RemoveFromRoleAsync(user, "Banned");
                }
                else
                {
                    return Json(new { success = false, message = "User is not banned" });
                }
                break;
        }

        if (result.Succeeded)
        {
            return Json(new { success = true });
        }
        
        return Json(new { success = false, message = result.Errors });
    }
}