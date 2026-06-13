using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TyreServiceApp.Models
{
    public class OrderComplexity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderComplexityId { get; set; }

        [Required]
        public int OrderNumber { get; set; }

        [Required]
        public int ComplexityCoefficientId { get; set; }

        [ForeignKey("OrderNumber")]
        public virtual Order Order { get; set; } = null!;

        [ForeignKey("ComplexityCoefficientId")]
        public virtual ComplexityCoefficient ComplexityCoefficient { get; set; } = null!;
    }
}
