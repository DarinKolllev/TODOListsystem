using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TODOlistsystem.Models;

namespace TODOlistsystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }
        public DbSet<Note> Notes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            builder.Entity<Note>(entity => {
                entity.HasIndex(n => n.UserId);
                entity.HasIndex(n => n.IsDeleted);
                entity.HasIndex(n => n.DueDate);
            });
        }
    }
}
