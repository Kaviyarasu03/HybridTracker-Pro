using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HybridTracker_Pro.Models
{
    public class ApplicationHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ApplicationId { get; set; }

        [ForeignKey("ApplicationId")]
        public Application? Application { get; set; }

        [Required]
        public string OldStatus { get; set; } = null!;

        [Required]
        public string NewStatus { get; set; } = null!;

        public string? Comments { get; set; }

        [Required]
        public string UpdatedByRole { get; set; } = null!; // "Admin", "BotMimic", "Applicant"

        public int? UpdatedByUserId { get; set; }

        [ForeignKey("UpdatedByUserId")]
        public User? UpdatedByUser { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}