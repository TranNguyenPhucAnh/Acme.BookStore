using System;
using Volo.Abp.Domain.Entities;

namespace Acme.BookStore.Schedulers
{
    public class Scheduler : Entity<Guid>
    {
        public virtual Guid RecipientEntityId { get; set; } = default!;

        public virtual string RecipientEntity { get; set; } = default!;

        public virtual string CronExpression { get; set; } = default!;

        public virtual RecipientTypeEnum RecipientType { get; set; }

        public virtual WorkerOutputTypeEnum WorkerOutputType { get; set; }

        public virtual string TimeZone { get; set; } = default!;
    }
}
