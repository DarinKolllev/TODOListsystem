using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TODOlistsystem.Models
{
    public class DailyStepLog
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string UserId { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        public int StepCount { get; set; } = 0;
        public int Goal { get; set; } = 10000;
    }
}
