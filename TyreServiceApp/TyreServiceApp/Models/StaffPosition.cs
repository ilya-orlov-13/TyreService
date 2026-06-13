using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models
{
    public class StaffPosition
    {
        [Key]
        public int StaffPositionId { get; set; }

        [Required(ErrorMessage = "Название должности обязательно")]
        [StringLength(50, ErrorMessage = "Название не более 50 символов")]
        [Display(Name = "Должность")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<AdminUser> AdminUsers { get; set; } = new List<AdminUser>();
    }
}
