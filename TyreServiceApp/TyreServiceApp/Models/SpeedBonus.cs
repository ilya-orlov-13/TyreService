using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Models
{
    public class SpeedBonus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SpeedBonusId { get; set; }

        [Required]
        public int MasterId { get; set; }

        [Required]
        public int OrderNumber { get; set; }

        [Required]
        public int WorkId { get; set; }

        [Required]
        [Range(0, 99999)]
        public int TimeSavedMin { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, 9999999.99)]
        public decimal BonusAmount { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = PermTime.Now;

        [ForeignKey("MasterId")]
        public virtual Master? Master { get; set; }

        [ForeignKey("OrderNumber")]
        public virtual Order? Order { get; set; }
    }
}
