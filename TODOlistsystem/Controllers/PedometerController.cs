using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TODOlistsystem.Data;
using TODOlistsystem.Models;

namespace TODOlistsystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PedometerController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedometerController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetTodaySteps()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            
            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                if (user.LastActiveDate == null || user.LastActiveDate.Value.Date < today.AddDays(-1))
                {
                    user.CurrentStreak = 1;
                }
                else if (user.LastActiveDate.Value.Date == today.AddDays(-1))
                {
                    user.CurrentStreak++;
                }
                
                if (user.LastActiveDate?.Date != today)
                {
                    user.LastActiveDate = today;
                    await _context.SaveChangesAsync();
                }
            }

            var stepLog = await _context.StepLogs
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == today);

            if (stepLog == null)
            {
                stepLog = new DailyStepLog
                {
                    UserId = userId,
                    Date = today,
                    StepCount = 0,
                    Goal = 10000
                };
                _context.StepLogs.Add(stepLog);
                await _context.SaveChangesAsync();
            }

            return Ok(new { 
                steps = stepLog.StepCount, 
                goal = stepLog.Goal,
                points = user?.Points ?? 0,
                streak = user?.CurrentStreak ?? 0
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddSteps([FromBody] AddStepsRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (request.Steps <= 0) return BadRequest(new { message = "Steps must be greater than zero." });

            var today = DateTime.UtcNow.Date;
            var stepLog = await _context.StepLogs
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == today);

            if (stepLog == null)
            {
                stepLog = new DailyStepLog
                {
                    UserId = userId,
                    Date = today,
                    StepCount = request.Steps,
                    Goal = 10000
                };
                _context.StepLogs.Add(stepLog);
            }
            else
            {
                stepLog.StepCount += request.Steps;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user != null)
            {
                // Simple gamification: 1 point per 1,000 steps logged.
                user.Points += Math.Max(1, request.Steps / 1000);
            }

            await _context.SaveChangesAsync();
            return Ok(new
            {
                steps = stepLog.StepCount,
                goal = stepLog.Goal,
                points = user?.Points ?? 0,
                streak = user?.CurrentStreak ?? 0
            });
        }

        [HttpPost("goal")]
        public async Task<IActionResult> SetDailyGoal([FromBody] SetGoalRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();
            if (request.Goal < 1000 || request.Goal > 50000)
            {
                return BadRequest(new { message = "Goal must be between 1000 and 50000." });
            }

            var today = DateTime.UtcNow.Date;
            var stepLog = await _context.StepLogs
                .FirstOrDefaultAsync(s => s.UserId == userId && s.Date.Date == today);

            if (stepLog == null)
            {
                stepLog = new DailyStepLog
                {
                    UserId = userId,
                    Date = today,
                    StepCount = 0,
                    Goal = request.Goal
                };
                _context.StepLogs.Add(stepLog);
            }
            else
            {
                stepLog.Goal = request.Goal;
            }

            await _context.SaveChangesAsync();
            return Ok(new { goal = stepLog.Goal, steps = stepLog.StepCount });
        }

        [HttpGet("weekly-summary")]
        public async Task<IActionResult> GetWeeklySummary()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var today = DateTime.UtcNow.Date;
            var fromDate = today.AddDays(-6);

            var logs = await _context.StepLogs
                .Where(s => s.UserId == userId && s.Date.Date >= fromDate && s.Date.Date <= today)
                .OrderBy(s => s.Date)
                .Select(s => new
                {
                    date = s.Date.ToString("yyyy-MM-dd"),
                    steps = s.StepCount,
                    goal = s.Goal
                })
                .ToListAsync();

            return Ok(logs);
        }
    }

    public class AddStepsRequest
    {
        public int Steps { get; set; }
    }

    public class SetGoalRequest
    {
        public int Goal { get; set; }
    }
}
