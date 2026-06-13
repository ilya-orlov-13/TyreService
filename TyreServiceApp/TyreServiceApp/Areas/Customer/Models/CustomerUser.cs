using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using TyreServiceApp.Models;

namespace TyreServiceApp.Areas.Customer.Models
{
    [Index(nameof(Phone), IsUnique = true)]
    public class CustomerUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        public string PinHash { get; set; } = string.Empty;

        public int? ClientId { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client? Client { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
