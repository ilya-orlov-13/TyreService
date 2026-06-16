using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Utils;

namespace TyreServiceApp.Models
{
    public class CompletedJobsPayout
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PayoutId { get; set; }

        [Required]
        public int OrderNumber { get; set; }

        [Required]
        public int MasterId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        [Range(0, 9999999.99)]
        public decimal Amount { get; set; }

        [Column(TypeName = "timestamp without time zone")]
        public DateTime CreatedAt { get; set; } = PermTime.Now;

        [ForeignKey("OrderNumber")]
        public virtual Order? Order { get; set; }

        [ForeignKey("MasterId")]
        public virtual Master? Master { get; set; }
    }
}
