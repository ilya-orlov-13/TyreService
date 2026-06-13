using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Worker.Models
{
    public class MasterUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MasterUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public int MasterId { get; set; }

        [ForeignKey("MasterId")]
        public virtual Master? Master { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
