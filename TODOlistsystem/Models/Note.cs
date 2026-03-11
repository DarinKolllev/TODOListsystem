using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TODOlistsystem.Models;

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

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey("UserId")]
    public virtual ApplicationUser User { get; set; } = null!;

    public string? SharedWithUserId { get; set; }
    public string? AssignedToUserId { get; set; }
}
