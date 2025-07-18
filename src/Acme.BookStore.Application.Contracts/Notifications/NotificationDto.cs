using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Notifications
{
    public class NotificationDto : AuditedEntityDto<Guid>
    {
        public Guid FromUserId { get; set; }
        public Guid ToUserId { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public string LocalizationKey { get; set; }
        public List<string> LocalizationArguments { get; set; }
        public string RedirectUrl { get; set; }
    }
}
