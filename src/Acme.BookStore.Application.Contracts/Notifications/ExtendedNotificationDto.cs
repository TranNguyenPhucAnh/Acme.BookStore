using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Notifications
{
    public class ExtendedNotificationDto
    {
        public int TotalUnread { get; set; }
        public PagedResultDto<NotificationDto> Result { get; set; }
    }
}
