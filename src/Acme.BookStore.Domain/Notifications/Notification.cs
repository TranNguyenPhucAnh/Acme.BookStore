using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities.Auditing;

namespace Acme.BookStore.Notifications
{
    public class Notification : AuditedEntity<Guid>
    {
        public string LocalizationKey { get; set; }
        public List<string> LocalizationArguments { get; set; }
        public string RedirectUrl { get; set; }
        public NotificationType Type { get; set; }
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public bool IsRead { get; set; }
    }
}