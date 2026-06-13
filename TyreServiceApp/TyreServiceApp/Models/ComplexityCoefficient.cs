using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class ComplexityCoefficient
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ComplexityCoefficientId { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Factor { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<OrderComplexity> OrderComplexities { get; set; } = new List<OrderComplexity>();
    }
}
