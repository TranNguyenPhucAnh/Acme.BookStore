using System;
using Volo.Abp.Domain.Entities;

namespace Acme.BookStore.Books
{
    public class BookMedias : Entity<Guid>
    {
        public Guid BookId { get; set; }
        public string FileName { get; set; } = default!;
        public string ObjectKey { get; set; } = default!;
        public long Size { get; set; }
    }
}
