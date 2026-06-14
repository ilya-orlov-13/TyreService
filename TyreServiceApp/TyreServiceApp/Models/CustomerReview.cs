using System.ComponentModel.DataAnnotations;
using TyreServiceApp.Utils;
using System.ComponentModel.DataAnnotations.Schema;
using TyreServiceApp.Areas.Customer.Models;

namespace TyreServiceApp.Models;

public class CustomerReview
{
    [Key]
    public int ReviewId { get; set; }

    public int CustomerId { get; set; }

    [ForeignKey(nameof(CustomerId))]
    public CustomerUser? Customer { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(1000)]
    public string Text { get; set; } = string.Empty;

    [StringLength(100)]
    public string? CarModel { get; set; }

    public bool IsApproved { get; set; }

    public int? OrderNumber { get; set; }

    [ForeignKey(nameof(OrderNumber))]
    public Order? Order { get; set; }

    public DateTime CreatedAt { get; set; } = PermTime.Now;

    public DateTime? UpdatedAt { get; set; }
}
