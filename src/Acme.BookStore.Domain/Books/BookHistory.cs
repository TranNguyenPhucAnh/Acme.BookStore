using System;
using Volo.Abp.Domain.Entities;

namespace Acme.BookStore.Books;

public class BookHistory : Entity<Guid>
{
    public Guid BookId { get; set; }
    
    public string ISBN { get; set; } = default!;

    public string Name { get; set; } = default!;

    public BookType Type { get; set; }

    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;

    public Guid AuthorId { get; set; }

    public ActionType ActionType { get; set; }

    public Guid? PreviousAuditId { get; set; }

    public int? Version { get; set; }

    public DateTime CreationTime { get; set; }

    public BookHistory()
    {

    }
}