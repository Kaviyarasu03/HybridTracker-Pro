using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HybridTracker_Pro.Data;
using HybridTracker_Pro.Models;

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

            return Ok(new
            {
                TotalApplications = totalApplications,
                TechnicalApplications = technicalApps,
                NonTechnicalApplications = nonTechnicalApps,
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

            if (!string.IsNullOrEmpty(request.Status))
                application.Status = request.Status;

            if (!string.IsNullOrEmpty(request.Comments))
                application.Comments = request.Comments;

            application.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Application updated successfully", applicationId = id });
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
    }

    public class ApplicationUpdateRequest
    {
        public string? Status { get; set; }
        public string? Comments { get; set; }
    }
}