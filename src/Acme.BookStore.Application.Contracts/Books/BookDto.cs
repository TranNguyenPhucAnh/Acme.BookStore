using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Books;

public class BookDto : AuditedEntityDto<Guid>
{
    public Guid AuthorId { get; set; }

    public string ISBN { get; set; } = default!;

    public string AuthorName { get; set; } = default!;

    public string Name { get; set; } = default!;

    public BookType Type { get; set; }

    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;
}
