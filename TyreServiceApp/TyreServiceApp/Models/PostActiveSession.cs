using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class PostActiveSession
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SessionId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int MasterId { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime StartedAt { get; set; } = DateTime.Now;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EndedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [ForeignKey("MasterId")]
        public virtual Master? Master { get; set; }
    }
}
