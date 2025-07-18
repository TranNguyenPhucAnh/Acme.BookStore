using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.Notifications
{
    public interface INotificationAppService : IApplicationService
    {
        Task InsertNotificationAndSendEmailAsync(
            Guid toUserId,
            string to,
            NotificationType type,
            INotifiableEntity entity,
            string crudAction);
        Task<ExtendedNotificationDto> GetNotificationListAsync(int skipCount, int maxResultCount);
        Task MarkAsReadAsync(List<Guid> ids);
        Task MarkAllAsReadAsync();
    }
}
