using Microsoft.EntityFrameworkCore;
using HybridTracker_Pro.Models;

namespace HybridTracker_Pro.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Application> Applications => Set<Application>();
        public DbSet<ApplicationHistory> ApplicationHistories => Set<ApplicationHistory>();
        public DbSet<JobPosting> JobPostings => Set<JobPosting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // REMOVED: HasData seeding - we'll do manual seeding in Program.cs
        }
    }
}