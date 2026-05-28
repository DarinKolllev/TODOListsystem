
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TODOlistsystem.Data;
using TODOlistsystem.Models;

namespace TODOlistsystem.Controllers;

[Authorize]
public class NotesController : Controller
{
    private readonly ApplicationDbContext _context;

    public NotesController(ApplicationDbContext context) => _context = context;

    // GET: Notes
    public async Task<IActionResult> Index() =>
        View(await _context.Notes
            .AsNoTracking()
            .Include(n => n.User)
            .Where(n => !n.IsDeleted)
            .ToListAsync());

    // GET: Notes/Details/5
    public async Task<IActionResult> Details(Guid id)
    {
        var note = await _context.Notes
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);

        return note == null ? NotFound() : View(note);
    }

    // GET: Notes/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Notes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Note note)
    {
        Console.WriteLine("POST ACTION HIT");
        // Check validation
        if (!ModelState.IsValid)
        {
            // Print validation errors to console for debugging
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }

            return View(note);
        }

        try
        {
            // Set default values
            note.Id = Guid.NewGuid();

            note.CreatedAt = DateTime.UtcNow;
            note.UpdatedAt = DateTime.UtcNow;

            note.IsDeleted = false;
            note.IsCompleted = false;
            note.IsActive = true;

            // Get current logged-in user
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            note.UserId = userId;

            // Save note
            _context.Notes.Add(note);

            await _context.SaveChangesAsync();

            Console.WriteLine("NOTE SAVED SUCCESSFULLY");

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            Console.WriteLine("SAVE ERROR:");
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.InnerException?.Message);

            return View(note);
        }
    }

    // GET: Notes/Edit/5
    public async Task<IActionResult> Edit(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        return note == null ? NotFound() : View(note);
    }

    // POST: Notes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, [Bind("Id,Content,IsCompleted,DueDate,Type")] Note model)
    {
        if (id != model.Id) return NotFound();

        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        var completedNow = !note.IsCompleted && model.IsCompleted;

        note.Content = model.Content;
        note.IsCompleted = model.IsCompleted;
        note.DueDate = model.DueDate;
        note.Type = model.Type;
        note.UpdatedAt = DateTime.UtcNow;

        if (completedNow)
        {
            var user = await _context.Users.FindAsync(note.UserId);
            if (user != null) user.Points += 10;
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Notes/Delete/5
    public async Task<IActionResult> Delete(Guid id)
    {
        var note = await _context.Notes
            .AsNoTracking()
            .Include(n => n.User)
            .FirstOrDefaultAsync(n => n.Id == id);

        return note == null ? NotFound() : View(note);
    }

    // POST: Notes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);

        if (note != null)
        {
            note.IsDeleted = true;
            note.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> ToggleComplete(Guid id)
    {
        var note = await _context.Notes.FindAsync(id);
        if (note == null) return NotFound();

        var completedNow = !note.IsCompleted;

        note.IsCompleted = completedNow;
        note.UpdatedAt = DateTime.UtcNow;

        if (completedNow)
        {
            var user = await _context.Users.FindAsync(note.UserId);
            if (user != null) user.Points += 10;
        }

        await _context.SaveChangesAsync();

        return Ok(new { note.IsCompleted });
    }

    [HttpPost]
    public async Task<IActionResult> QuickCreate([FromBody] QuickCreateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var note = new Note
        {
            Content = request.Content,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notes.Add(note);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            note.Id,
            note.Content
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetCategoryStats()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var stats = await _context.Notes
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .GroupBy(n => n.Type)
            .Select(g => new
            {
                Category = g.Key.ToString(),
                Count = g.Count()
            })
            .ToListAsync();

        return Ok(stats);
    }

    [HttpGet]
    public async Task<IActionResult> GetMonthlyProgress()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return Unauthorized();

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);

        var progress = (await _context.Notes
      .AsNoTracking()
      .Where(n =>
          n.UserId == userId &&
          n.IsCompleted &&
          n.UpdatedAt >= startOfMonth)
      .ToListAsync())
      .GroupBy(n => n.UpdatedAt!.Value.Date)
      .Select(g => new
      {
          Date = g.Key.ToString("yyyy-MM-dd"),
          Count = g.Count()
      })
      .OrderBy(g => g.Date)
      .ToList();

        return Ok(progress);
    }

    public class QuickCreateRequest
    {
        public string Content { get; set; } = string.Empty;
    }
}
