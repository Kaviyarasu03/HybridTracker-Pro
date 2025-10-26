using System.ComponentModel.DataAnnotations;

namespace HybridTracker_Pro.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        // Navigation property
        public ICollection<User>? Users { get; set; }
    }
}