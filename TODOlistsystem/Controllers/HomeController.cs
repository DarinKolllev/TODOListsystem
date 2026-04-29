using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TODOlistsystem.Models;

namespace TODOlistsystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data.ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, Data.ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = System.Security.Claims.ClaimTypes.NameIdentifier;
                var currentUserId = User.FindFirst(userId)?.Value;
                
                if (currentUserId != null)
                {
                    var tasks = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(_context.Notes.Where(n => n.UserId == currentUserId && !n.IsDeleted));
                    ViewBag.TotalTasks = tasks.Count;
                    ViewBag.CompletedTasks = tasks.Count(t => t.IsCompleted);
                    ViewBag.PendingTasks = tasks.Count(t => !t.IsCompleted);
                }
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
