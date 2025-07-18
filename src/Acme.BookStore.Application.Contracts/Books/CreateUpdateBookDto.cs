using System;
using System.ComponentModel.DataAnnotations;

namespace Acme.BookStore.Books;

public class CreateUpdateBookDto
{
    public Guid AuthorId { get; set; }

    public string AuthorName { get; set; }

    [StringLength(128)]
    public string Name { get; set; } = string.Empty;

    public BookType Type { get; set; } = BookType.UNDEFINED;

    public string ISBN { get; set; } = default!;

    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;
}
