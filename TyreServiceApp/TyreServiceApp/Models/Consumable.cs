using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class Consumable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ConsumableId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostPrice { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal SellPrice { get; set; }

        public virtual ICollection<OrderConsumable> OrderConsumables { get; set; } = new List<OrderConsumable>();
    }
}
