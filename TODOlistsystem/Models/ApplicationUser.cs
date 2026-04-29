using Microsoft.AspNetCore.Identity;
using System;

namespace TODOlistsystem.Models {
    public class ApplicationUser : IdentityUser {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int Points { get; set; } = 0;
        public int CurrentStreak { get; set; } = 0;
        public DateTime? LastActiveDate { get; set; }

        public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
