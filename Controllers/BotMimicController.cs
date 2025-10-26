using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HybridTracker_Pro.Data;
using HybridTracker_Pro.Models;

namespace HybridTracker_Pro.Controllers
{
    [Authorize(Roles = "BotMimic,Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class BotMimicController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BotMimicController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/bot/update-all
        [HttpPost("update-all")]
        public async Task<IActionResult> UpdateAllTechnicalApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.User)
                .Where(a => string.Equals(a.RoleApplied, "technical", StringComparison.OrdinalIgnoreCase)
                         && a.Status != "Offer" && a.Status != "Rejected")
                .ToListAsync();

            var updatedCount = 0;

            foreach (var app in applications)
            {
                var oldStatus = app.Status;
                app.Status = app.Status switch
                {
                    "Submitted" => "Reviewed",
                    "Reviewed" => "Interview",
                    "Interview" => "Offer",
                    _ => app.Status
                };

                if (oldStatus != app.Status)
                {
                    app.Comments = $"Bot Mimic: Auto-updated from {oldStatus} to {app.Status}";
                    app.UpdatedAt = DateTime.UtcNow;
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Technical applications updated successfully",
                updatedCount,
                totalProcessed = applications.Count
            });
        }

        // POST: api/bot/update/{id}
        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateSpecificApplication(int id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id
                    && string.Equals(a.RoleApplied, "technical", StringComparison.OrdinalIgnoreCase));

            if (application == null)
                return NotFound(new { message = "Technical application not found" });

            if (application.Status == "Offer" || application.Status == "Rejected")
                return BadRequest(new { message = "Cannot update application in final status" });

            var oldStatus = application.Status;
            application.Status = application.Status switch
            {
                "Submitted" => "Reviewed",
                "Reviewed" => "Interview",
                "Interview" => "Offer",
                _ => application.Status
            };

            application.Comments = $"Bot Mimic: Auto-updated from {oldStatus} to {application.Status}";
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Application updated successfully",
                applicationId = id,
                oldStatus,
                newStatus = application.Status
            });
        }

        // GET: api/bot/status
        [HttpGet("status")]
        public async Task<IActionResult> GetBotStatus()
        {
            var technicalApps = await _context.Applications
                .Where(a => string.Equals(a.RoleApplied, "technical", StringComparison.OrdinalIgnoreCase))
                .ToListAsync();

            var stats = technicalApps
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var pendingUpdates = technicalApps.Count(a => a.Status != "Offer" && a.Status != "Rejected");

            return Ok(new
            {
                TotalTechnicalApplications = technicalApps.Count,
                PendingUpdates = pendingUpdates,
                StatusBreakdown = stats,
                LastUpdated = DateTime.UtcNow
            });
        }

        // GET: api/bot/statistics
        [HttpGet("statistics")]
        public async Task<IActionResult> GetStatistics()
        {
            var allApplications = await _context.Applications.ToListAsync();

            var technicalStats = allApplications
                .Where(a => string.Equals(a.RoleApplied, "technical", StringComparison.OrdinalIgnoreCase))
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            var nonTechnicalStats = allApplications
                .Where(a => string.Equals(a.RoleApplied, "non-technical", StringComparison.OrdinalIgnoreCase))
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToList();

            return Ok(new
            {
                TechnicalApplications = new
                {
                    Total = technicalStats.Sum(t => t.Count),
                    Breakdown = technicalStats
                },
                NonTechnicalApplications = new
                {
                    Total = nonTechnicalStats.Sum(t => t.Count),
                    Breakdown = nonTechnicalStats
                },
                OverallTotal = allApplications.Count
            });
        }

        // POST: api/bot/reset/{id}
        [HttpPost("reset/{id}")]
        public async Task<IActionResult> ResetApplicationStatus(int id)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound(new { message = "Application not found" });

            application.Status = "Submitted";
            application.Comments = "Bot Mimic: Status reset to Submitted";
            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Application status reset successfully",
                applicationId = id,
                newStatus = application.Status
            });
        }
    }
}