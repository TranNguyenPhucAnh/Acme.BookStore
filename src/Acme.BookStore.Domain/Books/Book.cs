using System;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.BookStore.Books;

public class Book : AuditedAggregateRoot<Guid>
{
    public string ISBN { get; set; } = default!;

    public string Name { get; set; } = default!;

    public BookType Type { get; set; }

    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;

    public  Guid AuthorId { get; set; }

    public Book()
    {

    }
}