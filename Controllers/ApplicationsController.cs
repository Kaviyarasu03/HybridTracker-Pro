using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using HybridTracker_Pro.Data;
using HybridTracker_Pro.Models;
using HybridTracker_Pro.Services;
using System.Security.Claims;

namespace HybridTracker_Pro.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApplicationsController(AppDbContext context)
        {
            _context = context;
        }

        // POST: api/applications
        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] Application application)
        {
            if (application == null)
                return BadRequest(new { message = "Application data is required" });

            var user = await _context.Users.FindAsync(application.UserId);
            if (user == null)
                return BadRequest(new { message = "Invalid UserId" });

            // Default status if not provided
            if (string.IsNullOrEmpty(application.Status))
                application.Status = "Submitted";

            application.CreatedAt = DateTime.UtcNow;
            application.UpdatedAt = DateTime.UtcNow;

            _context.Applications.Add(application);
            await _context.SaveChangesAsync();

            // ADD HISTORY LOGGING
            using (var scope = HttpContext.RequestServices.CreateScope())
            {
                var historyService = scope.ServiceProvider.GetRequiredService<HistoryService>();
                await historyService.LogApplicationHistory(
                    application.Id,
                    "None",
                    application.Status,
                    "Application created",
                    "Applicant",
                    application.UserId
                );
            }

            return Ok(new
            {
                message = "Application created successfully",
                application = new
                {
                    application.Id,
                    application.CandidateName,
                    application.RoleApplied,
                    application.Status,
                    application.Comments,
                    User = new { user.Id, user.Username, user.Email }
                }
            });
        }

        // GET: api/applications/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserApplications(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound(new { message = "User not found" });

            var applications = await _context.Applications
                .Where(a => a.UserId == userId)
                .Select(a => new
                {
                    a.Id,
                    a.CandidateName,
                    a.RoleApplied,
                    a.Status,
                    a.Comments,
                    a.CreatedAt
                })
                .ToListAsync();

            return Ok(new { User = new { user.Id, user.Username, user.Email }, Applications = applications });
        }

        // GET: api/applications
        [HttpGet]
        [Authorize(Roles = "Admin,BotMimic")]
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

        // PUT: api/applications/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] ApplicationUpdateModel request)
        {
            var application = await _context.Applications.FindAsync(id);
            if (application == null)
                return NotFound(new { message = "Application not found" });

            // Users can only update their own applications
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            if (application.UserId != userId)
                return StatusCode(403, new { message = "Can only update your own applications" });

            // Users cannot update technical applications (only BotMimic can)
            if (application.RoleApplied.ToLower() == "technical")
                return BadRequest(new { message = "Technical applications are automatically updated by BotMimic" });

            var oldStatus = application.Status;

            // Update only provided fields
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
                await historyService.LogApplicationHistory(
                    application.Id,
                    oldStatus,
                    application.Status,
                    request.Comments ?? "Status updated",
                    "Applicant",
                    userId
                );
            }

            return Ok(new { message = "Application updated successfully", applicationId = id });
        }

        // GET: api/applications/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplication(int id)
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
                application.UpdatedAt,
                User = new { application.User.Id, application.User.Username, application.User.Email }
            });
        }
    }

   
}