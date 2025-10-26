using HybridTracker_Pro.Data;
using HybridTracker_Pro.Models;

namespace HybridTracker_Pro.Services
{
    public class HistoryService
    {
        private readonly AppDbContext _context;

        public HistoryService(AppDbContext context)
        {
            _context = context;
        }

        public async Task LogApplicationHistory(int applicationId, string oldStatus, string newStatus,
            string comments, string updatedByRole, int? updatedByUserId = null)
        {
            var history = new ApplicationHistory
            {
                ApplicationId = applicationId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Comments = comments,
                UpdatedByRole = updatedByRole,
                UpdatedByUserId = updatedByUserId,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ApplicationHistories.Add(history);
            await _context.SaveChangesAsync();
        }
    }
}