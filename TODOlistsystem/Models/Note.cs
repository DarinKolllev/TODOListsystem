using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TODOlistsystem.Models;

namespace TODOlistsystem.Models
{
    public enum TaskType
    {
        General,
        Work,
        Fitness,
        Reading,
        Personal,
        Health,
        Challenge
    }

    public enum TaskPriority
    {
        Low,
        Medium,
        High,
        Urgent
    }

    public class Note
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(5000)]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public bool IsCompleted { get; set; } = false;
        public bool IsDeleted { get; set; } = false;
        
        public TaskType Type { get; set; } = TaskType.General;
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DueDate { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public string? SharedWithUserId { get; set; }
        public string? AssignedToUserId { get; set; }
    }
}
