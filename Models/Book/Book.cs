using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Models.Book;

[Index("Isbn", Name = "UQ__Books__447D36EA3D262D66", IsUnique = true)]
[Index("BookSid", Name = "UQ__Books__EEC908F1728795D2", IsUnique = true)]
public partial class Book
{
    [Key]
    public int BookId { get; set; }

    [Column("BookSID")]
    [StringLength(100)]
    public string BookSid { get; set; } = null!;

    [StringLength(200)]
    [Unicode(false)]
    public string Title { get; set; } = null!;

    [StringLength(150)]
    [Unicode(false)]
    public string Author { get; set; } = null!;

    [Column("ISBN")]
    [StringLength(20)]
    [Unicode(false)]
    public string? Isbn { get; set; }

    public int? PublishedYear { get; set; }

    public int IsAvailable { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    public int Status { get; set; }
}
