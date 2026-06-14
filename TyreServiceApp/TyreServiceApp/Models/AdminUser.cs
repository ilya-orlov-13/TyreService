using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Models
{
    public class AdminUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AdminUserId { get; set; }

        [Required]
        [StringLength(50)]
        public string Login { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? FullName { get; set; }

        [Display(Name = "Должность")]
        public int? StaffPositionId { get; set; }

        [ForeignKey(nameof(StaffPositionId))]
        public virtual StaffPosition? StaffPosition { get; set; }

        public DateTime CreatedAt { get; set; } = PermTime.Now;
    }
}
