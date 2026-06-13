using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class OwnerSetting
    {
        [Key]
        public int OwnerSettingId { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal AcquiringFeePercent { get; set; } = 2m;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal TaxPercent { get; set; } = 6m;

        [StringLength(200)]
        public string? Name { get; set; }
    }
}
