namespace Models.RequestModel;
public class BookRequestModel
{
    public string Title { get; set; }
    public string Author { get; set; }
    public string? Isbn { get; set; }
    public int? PublishedYear { get; set; } 
}