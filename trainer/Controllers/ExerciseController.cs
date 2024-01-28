using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using trainer.Data;
using trainer.Models;

namespace trainer.Controllers
{
    [Authorize (Roles = "Admin, SuperAdmin", Policy = "NotBanned")]
    public class ExerciseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ExerciseController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Exercise
        public async Task<IActionResult> Index()
        {
            return _context.Exercises != null ? 
                  View(await _context.Exercises.ToListAsync()) :
                  Problem("Entity set 'ApplicationDbContext.Exercises'  is null.");
        }

        // GET: Exercise/Create
        public IActionResult Create()
        {
            // Gets a list of all the muscle groups in the database to pass to the view
            var muscleGroups = _context.Exercises.Select(e => e.MuscleGroup).Distinct().ToList();
            ViewBag.MuscleGroups = new SelectList(muscleGroups);
            
            return View();
        }

        // POST: Exercise/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,MuscleGroup,RequiresGym")] Exercise exercise, IFormFile gifFile)
        {
            ModelState.Remove("GifUrl");

            if (gifFile == null)
            {
                ModelState.AddModelError("GifUrl", "This field is required. Please upload file.");
            }
            else if (gifFile.ContentType.ToLower() != "image/gif")
            {
                ModelState.AddModelError("GiftUrl", "The file must be a .gif file.");
            }
            else if (ModelState.IsValid)
            {
                
                var fileName = Path.GetFileName(gifFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "img", "gifs", fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await gifFile.CopyToAsync(fileStream);
                }

                // Save only the file name in the database
                exercise.GifUrl = fileName;
                
                _context.Add(exercise);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(exercise);
        }

        // GET: Exercise/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Exercises == null)
            {
                return NotFound();
            }

            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null)
            {
                return NotFound();
            }
            
            // Gets a list of all the muscle groups in the database to pass to the view
            var muscleGroups = _context.Exercises.Select(e => e.MuscleGroup).Distinct().ToList();
            ViewBag.MuscleGroups = new SelectList(muscleGroups);
            
            return View(exercise);
        }

        // POST: Exercise/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,MuscleGroup,RequiresGym,GifUrl")] Exercise exercise)
        {
            if (id != exercise.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(exercise);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ExerciseExists(exercise.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(exercise);
        }

        // GET: Exercise/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Exercises == null)
            {
                return NotFound();
            }

            var exercise = await _context.Exercises
                .FirstOrDefaultAsync(m => m.Id == id);
            if (exercise == null)
            {
                return NotFound();
            }

            return View(exercise);
        }

        // POST: Exercise/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Exercises == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Exercises'  is null.");
            }
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise != null)
            {
                _context.Exercises.Remove(exercise);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ExerciseExists(int id)
        {
          return (_context.Exercises?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
