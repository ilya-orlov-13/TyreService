using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Utils;

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
        public DateTime StartedAt { get; set; } = PermTime.Now;

        [Column(TypeName = "timestamp without time zone")]
        public DateTime? EndedAt { get; set; }

        [ForeignKey("PostId")]
        public virtual Post? Post { get; set; }

        [ForeignKey("MasterId")]
        public virtual Master? Master { get; set; }
    }
}
