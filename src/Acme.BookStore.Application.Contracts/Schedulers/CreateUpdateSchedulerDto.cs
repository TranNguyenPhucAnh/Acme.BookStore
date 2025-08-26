using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Schedulers
{
    public class CreateUpdateSchedulerDto : EntityDto<Guid>
    {
        public Guid RecipientEntityId { get; set; } = default!;
        public string RecipientEntity { get; set; } = default!;
        public string CronExpression { get; set; } = default!;
        public virtual RecipientTypeEnum RecipientType { get; set; }
        public virtual WorkerOutputTypeEnum WorkerOutputType { get; set; }
        public string TimeZone { get; set; } = default!;
    }
}
