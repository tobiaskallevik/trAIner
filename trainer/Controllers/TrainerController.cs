using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers;

[Authorize (Policy = "NotBanned")]
public class TrainerController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}