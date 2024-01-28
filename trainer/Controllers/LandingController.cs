using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers;

public class LandingController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    public LandingController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }
   
    public async Task<IActionResult> LandingPage()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            ModelState.AddModelError(string.Empty, "User not found.");
            return View();
        }
        return RedirectToAction("Index", "Trainer");
    }
}