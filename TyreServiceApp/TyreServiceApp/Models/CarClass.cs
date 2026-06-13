using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class CarClass
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CarClassId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        [Display(Name = "Базовая стоимость")]
        public decimal BaseTariff { get; set; }

        public int SortOrder { get; set; }

        public virtual ICollection<Car> Cars { get; set; } = new List<Car>();
        public virtual ICollection<ServiceTariff> ServiceTariffs { get; set; } = new List<ServiceTariff>();
    }
}
