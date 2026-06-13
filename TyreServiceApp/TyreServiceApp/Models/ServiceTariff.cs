using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class ServiceTariff
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiceTariffId { get; set; }

        [Required]
        public int ServiceCode { get; set; }

        [Required]
        public int CarClassId { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal BasePrice { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal MasterSharePercent { get; set; } = 40m;

        [ForeignKey("ServiceCode")]
        public virtual Service Service { get; set; } = null!;

        [ForeignKey("CarClassId")]
        public virtual CarClass CarClass { get; set; } = null!;
    }
}
