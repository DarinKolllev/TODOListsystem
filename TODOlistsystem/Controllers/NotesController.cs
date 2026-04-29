
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TODOlistsystem.Data;
using TODOlistsystem.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace TODOlistsystem.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public NotesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Notes
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Notes.Include(n => n.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Notes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // GET: Notes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Notes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,DueDate,Type")] Note note)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            note.Id = Guid.NewGuid();
            note.UserId = userId;
            note.CreatedAt = DateTime.UtcNow;
            note.IsActive = true;
            note.IsCompleted = false;
            note.IsDeleted = false;

            ModelState.Remove("UserId");
            ModelState.Remove("User");

            if (ModelState.IsValid)
            {
                _context.Add(note);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(note);
        }

        // GET: Notes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes.FindAsync(id);
            if (note == null)
            {
                return NotFound();
            }
            return View(note);
        }

        // POST: Notes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Content,IsCompleted,DueDate,Type")] Note note)
        {
            if (id != note.Id)
            {
                return NotFound();
            }

            var existingNote = await _context.Notes.FindAsync(id);
            if (existingNote == null)
            {
                return NotFound();
            }

            existingNote.Content = note.Content;
            
            if (!existingNote.IsCompleted && note.IsCompleted)
            {
                var user = await _context.Users.FindAsync(existingNote.UserId);
                if (user != null)
                {
                    user.Points += 10;
                }
            }
            
            existingNote.IsCompleted = note.IsCompleted;
            existingNote.DueDate = note.DueDate;
            existingNote.Type = note.Type;
            existingNote.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!NoteExists(note.Id))
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

        // GET: Notes/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var note = await _context.Notes
                .Include(n => n.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (note == null)
            {
                return NotFound();
            }

            return View(note);
        }

        // POST: Notes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note != null)
            {
                _context.Notes.Remove(note);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleComplete(Guid id)
        {
            var note = await _context.Notes.FindAsync(id);
            if (note == null) return NotFound();

            bool wasCompleted = note.IsCompleted;
            note.IsCompleted = !note.IsCompleted;
            note.UpdatedAt = DateTime.UtcNow;

            if (!wasCompleted && note.IsCompleted)
            {
                var user = await _context.Users.FindAsync(note.UserId);
                if (user != null) user.Points += 10;
            }

            await _context.SaveChangesAsync();
            return Ok(new { isCompleted = note.IsCompleted });
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreate([FromBody] QuickCreateRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content)) return BadRequest();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var note = new Note
            {
                Content = request.Content,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Type = TaskType.General,
                Priority = TaskPriority.Medium
            };

            _context.Notes.Add(note);
            await _context.SaveChangesAsync();

            return Ok(new { id = note.Id, content = note.Content, type = note.Type.ToString(), priority = note.Priority.ToString() });
        }

        [HttpGet]
        public async Task<IActionResult> GetCategoryStats()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var stats = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .GroupBy(n => n.Type)
                .Select(g => new { category = g.Key.ToString(), count = g.Count() })
                .ToListAsync();

            return Ok(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetMonthlyProgress()
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            
            var completedTasks = await _context.Notes
                .Where(n => n.UserId == userId && n.IsCompleted && n.UpdatedAt >= startOfMonth)
                .GroupBy(n => n.UpdatedAt.Value.Date)
                .Select(g => new { date = g.Key.ToString("yyyy-MM-dd"), count = g.Count() })
                .OrderBy(g => g.date)
                .ToListAsync();

            return Ok(completedTasks);
        }

        public class QuickCreateRequest { public string Content { get; set; } }

        private bool NoteExists(Guid id)
        {
            return _context.Notes.Any(e => e.Id == id);
        }
    }
}
