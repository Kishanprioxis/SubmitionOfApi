using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Models.RequestModel;
public class BookRequestModel
{
    [Required]
    public string Title { get; set; }
    [Required]
    public string Author { get; set; }
    [Required]
    public string? Isbn { get; set; }
    [Required]
    public int? PublishedYear { get; set; } 
}