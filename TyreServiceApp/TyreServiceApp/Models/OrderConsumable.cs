using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class OrderConsumable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderConsumableId { get; set; }

        [Required]
        public int OrderNumber { get; set; }

        [Required]
        public int ConsumableId { get; set; }

        [Range(1, 999)]
        public int Quantity { get; set; } = 1;

        [ForeignKey("OrderNumber")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ConsumableId")]
        public virtual Consumable Consumable { get; set; } = null!;
    }
}
