namespace TyreServiceApp.Models.Api;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public string Author { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CarModel { get; set; }
    public int? OrderNumber { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewRequest
{
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? CarModel { get; set; }
    public int? OrderNumber { get; set; }
}
