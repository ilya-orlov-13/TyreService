using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class WorkTimeLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int WorkId { get; set; }

        [Required]
        public int MasterId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime StartTime { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EndTime { get; set; }

        public int DurationMinutes { get; set; }

        [ForeignKey("WorkId")]
        public virtual CompletedWork CompletedWork { get; set; } = null!;

        [ForeignKey("MasterId")]
        public virtual Master Master { get; set; } = null!;
    }
}
