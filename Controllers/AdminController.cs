using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HybridTracker_Pro.Data;
using HybridTracker_Pro.Models;
using HybridTracker_Pro.Services;
using System.Security.Claims;

namespace HybridTracker_Pro.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/admin/dashboard
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var totalApplications = await _context.Applications.CountAsync();
            var technicalApps = await _context.Applications.CountAsync(a => a.RoleApplied.ToLower() == "technical");
            var nonTechnicalApps = await _context.Applications.CountAsync(a => a.RoleApplied.ToLower() == "non-technical");
            var totalJobPostings = await _context.JobPostings.CountAsync(j => j.IsActive);

            return Ok(new
            {
                TotalApplications = totalApplications,
                TechnicalApplications = technicalApps,
                NonTechnicalApplications = nonTechnicalApps,
                ActiveJobPostings = totalJobPostings,
                PendingApplications = await _context.Applications.CountAsync(a => a.Status == "Submitted")
            });
        }

        // GET: api/admin/applications
        [HttpGet("applications")]
        public async Task<IActionResult> GetAllApplications()
        {
            var applications = await _context.Applications
                .Include(a => a.User)
                .Select(a => new
                {
                    a.Id,
                    a.CandidateName,
                    a.RoleApplied,
                    a.Status,
                    a.Comments,
                    a.CreatedAt,
                    User = new { a.User.Id, a.User.Username, a.User.Email }
                })
                .ToListAsync();

            return Ok(applications);
        }

        // GET: api/admin/applications/5
        [HttpGet("applications/{id}")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            var application = await _context.Applications
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application == null)
                return NotFound(new { message = "Application not found" });

            return Ok(new
            {
                application.Id,
                application.CandidateName,
                application.RoleApplied,
                application.Status,
                application.Comments,
                application.CreatedAt,
                User = new { application.User.Id, application.User.Username, application.User.Email }
            });
        }

        // PUT: api/admin/applications/5
        [HttpPut("applications/{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationUpdateRequest request)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound(new { message = "Application not found" });

            // FIX: Admin can only update NON-TECHNICAL applications
            if (application.RoleApplied.ToLower() == "technical")
                return BadRequest(new { message = "Admin can only update non-technical applications. Use BotMimic for technical roles." });

            var oldStatus = application.Status;

            if (!string.IsNullOrEmpty(request.Status))
                application.Status = request.Status;

            if (!string.IsNullOrEmpty(request.Comments))
                application.Comments = request.Comments;

            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // ADD HISTORY LOGGING
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var historyService = scope.ServiceProvider.GetRequiredService<HistoryService>();
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
                await historyService.LogApplicationHistory(
                    application.Id,
                    oldStatus,
                    application.Status,
                    request.Comments ?? "Admin updated status",
                    "Admin",
                    userId
                );
            }

            return Ok(new { message = "Non-technical application updated successfully", applicationId = id });
        }

        // GET: api/admin/stats
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.Applications
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return Ok(stats);
        }

        // POST: api/admin/jobpostings
        [HttpPost("jobpostings")]
        public async Task<IActionResult> CreateJobPosting([FromBody] JobPosting jobPosting)
        {
            if (jobPosting == null)
                return BadRequest(new { message = "Job posting data is required" });

            // Set created by user (current admin)
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            jobPosting.CreatedByUserId = userId;
            jobPosting.CreatedAt = DateTime.UtcNow;

            _context.JobPostings.Add(jobPosting);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Job posting created successfully",
                jobPosting = new
                {
                    jobPosting.Id,
                    jobPosting.Title,
                    jobPosting.Description,
                    jobPosting.Department,
                    jobPosting.IsActive
                }
            });
        }

        // GET: api/admin/jobpostings
        [HttpGet("jobpostings")]
        public async Task<IActionResult> GetAllJobPostings()
        {
            var jobPostings = await _context.JobPostings
                .Include(j => j.CreatedByUser)
                .Where(j => j.IsActive)
                .Select(j => new
                {
                    j.Id,
                    j.Title,
                    j.Description,
                    j.Department,
                    j.IsActive,
                    j.CreatedAt,
                    CreatedBy = new { j.CreatedByUser.Id, j.CreatedByUser.Username }
                })
                .ToListAsync();

            return Ok(jobPostings);
        }

        // GET: api/admin/application-history/{applicationId}
        [HttpGet("application-history/{applicationId}")]
        public async Task<IActionResult> GetApplicationHistory(int applicationId)
        {
            var history = await _context.ApplicationHistories
                .Include(h => h.UpdatedByUser)
                .Where(h => h.ApplicationId == applicationId)
                .OrderByDescending(h => h.UpdatedAt)
                .Select(h => new
                {
                    h.Id,
                    h.OldStatus,
                    h.NewStatus,
                    h.Comments,
                    h.UpdatedByRole,
                    h.UpdatedAt,
                    UpdatedBy = h.UpdatedByUser != null ?
                        new { h.UpdatedByUser.Id, h.UpdatedByUser.Username } : null
                })
                .ToListAsync();

            return Ok(history);
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _context.Roles.ToListAsync();
            return Ok(roles);
        }
    }

   
}