using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TODOlistsystem.Models;

namespace TODOlistsystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Data.ApplicationDbContext _context;

        public HomeController(
            ILogger<HomeController> logger,
            Data.ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(currentUserId))
                {
                    var baseQuery = _context.Notes
                        .AsNoTracking()
                        .Where(n => n.UserId == currentUserId && !n.IsDeleted);

                    var totalTasks = await baseQuery.CountAsync();

                    var completedTasks = await baseQuery
                        .CountAsync(n => n.IsCompleted);

                    ViewBag.TotalTasks = totalTasks;
                    ViewBag.CompletedTasks = completedTasks;
                    ViewBag.PendingTasks = totalTasks - completedTasks;
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}