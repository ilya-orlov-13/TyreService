using System.ComponentModel.DataAnnotations;

namespace TyreServiceApp.Models
{
    public class Position
    {
        [Key]
        public int PositionId { get; set; }

        [Required(ErrorMessage = "Название должности обязательно")]
        [StringLength(50, ErrorMessage = "Название не более 50 символов")]
        [Display(Name = "Должность")]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Master> Masters { get; set; } = new List<Master>();
    }
}
