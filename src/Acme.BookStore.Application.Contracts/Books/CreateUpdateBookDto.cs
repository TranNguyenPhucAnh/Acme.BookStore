using System;
using System.ComponentModel.DataAnnotations;

namespace Acme.BookStore.Books;

public class CreateUpdateBookDto
{
    public Guid AuthorId { get; set; }

    [Required]
    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public BookType Type { get; set; } = BookType.Undefined;

    public string ISBN { get; set; } = default!;

    [Required]
    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;
}
