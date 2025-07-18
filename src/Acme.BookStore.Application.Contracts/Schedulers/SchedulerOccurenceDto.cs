using System;

namespace Acme.BookStore.Schedulers
{
    public class SchedulerOccurenceDto : SchedulerDto
    {
        public DateTime NextOccurrence { get; set; }
        public SchedulerOccurenceDto(
            Guid recipientEntityId,
            string recipientEntity,
            RecipientTypeEnum typeEnum,
            WorkerOutputTypeEnum outputTypeEnum,
            DateTime nextOccurrence
            )
        {
            RecipientEntityId = recipientEntityId;
            RecipientEntity = recipientEntity;
            RecipientType = typeEnum;
            WorkerOutputType = outputTypeEnum;
            NextOccurrence = nextOccurrence;
        }
    }
}
