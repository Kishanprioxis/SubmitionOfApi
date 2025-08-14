namespace Models.ResponseModel;

public class BookResponseModel
{
    
    public string BookSid { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string? Isbn { get; set; }
    public int? PublishedYear { get; set; }
    public int IsAvailable { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}