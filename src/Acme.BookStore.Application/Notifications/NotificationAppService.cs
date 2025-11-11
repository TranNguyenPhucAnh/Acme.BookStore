using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Emailing.Templates;
using Volo.Abp.TextTemplating;
using Volo.Abp.Users;
using static Acme.BookStore.Commons.UrlHelper;

namespace Acme.BookStore.Notifications
{
    public class NotificationAppService(
        ICurrentUser currentUser,
        IEmailSender emailSender,
        IRepository<Notification, Guid> notificationRepository,
        ITemplateRenderer templateRenderer,
        ILogger<NotificationAppService> logger
        ) : BookStoreAppService, INotificationAppService
    {
        private readonly ICurrentUser _currentUser = currentUser;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly IRepository<Notification, Guid> _notificationRepository = notificationRepository;
        private readonly ITemplateRenderer _templateRenderer = templateRenderer;
        private readonly ILogger<NotificationAppService> _logger = logger;

        public async Task InsertNotificationAndSendEmailAsync(
            Guid toUserId,
            string to,
            NotificationType type,
            INotifiableEntity entity,
            string crudAction)
        {
            //persist notification
            await _notificationRepository.InsertAsync(
                new Notification
                {
                    FromUserId = _currentUser.Id.GetValueOrDefault(),
                    ToUserId = toUserId,
                    Type = type,
                    IsRead = false,
                    LocalizationKey = entity.GetLocalizationKey("Notification", type),
                    LocalizationArguments = entity.GetLocalizationNotificationArgs(crudAction).Select(arg => arg.ToString()).ToList(),
                    RedirectUrl = GetEntityUrl(entity.GetType())
                }
            );

            //Send email
            var subject = L[entity.GetLocalizationKey("Subject", type), entity.GetLocalizationSubjectArgs(crudAction)];

            _logger.LogInformation("Sending email with subject: {Subject}", subject);

            var model = new EmailTemplateModel
            {
                DistributionDomainName = BookStoreConfigurations.AwsCloudFrontDomain,
                Year = DateTime.UtcNow.Year,
                LocalizedMessage = L[entity.GetLocalizationKey("Body", type), entity.GetLocalizationBodyArgs(crudAction)]
            };

            var args = entity.GetLocalizationBodyArgs(crudAction);

            _logger.LogInformation("Email model rendered successfully: {key}", model.LocalizedMessage);

            var body = await _templateRenderer.RenderAsync(StandardEmailTemplates.Message, globalContext: new Dictionary<string, object>
            {
                { "model", model }
            });

            _logger.LogInformation("Email body rendered successfully: {Body}", body);

            await _emailSender.QueueAsync(to, subject, body);
        }

        public async Task<ExtendedNotificationDto> GetNotificationListAsync(int skipCount, int maxResultCount)
        {
            var query = (await _notificationRepository.GetQueryableAsync()).Where(n => n.ToUserId == _currentUser.Id);
            var totalUnread = query.Count(n => !n.IsRead);

            var items = query
                    .OrderByDescending(n => n.CreationTime)
                    .PageBy(skipCount, maxResultCount)
                    .Select(n => new NotificationDto
                    {
                        Id = n.Id,
                        FromUserId = n.FromUserId,
                        ToUserId = n.ToUserId,
                        Type = n.Type,
                        IsRead = n.IsRead,
                        LocalizationKey = n.LocalizationKey,
                        LocalizationArguments = n.LocalizationArguments,
                        RedirectUrl = n.RedirectUrl,
                        CreationTime = n.CreationTime,
                        LastModificationTime = n.LastModificationTime
                    }).ToList();

            return new ExtendedNotificationDto
            {
                TotalUnread = totalUnread,
                Result = new PagedResultDto<NotificationDto>
                {
                    Items = items,
                    TotalCount = query.Count()
                }
            };
        }

        public async Task MarkAsReadAsync(List<Guid> ids)
        {
            if (ids == null || ids.Count == 0)
            {
                _logger.LogInformation("No notification IDs provided for marking as read.");
                return;
            }
            var query = await _notificationRepository.GetQueryableAsync();
            await query.Where(n => n.ToUserId == _currentUser.Id && ids.Contains(n.Id))
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

            _logger.LogInformation("Marking notifications as read: {Ids}", string.Join(", ", ids));
        }

        public async Task MarkAllAsReadAsync()
        {
            var query = await _notificationRepository.GetQueryableAsync();
            //ExecuteUpdateAsync tạo truy vấn SQL và thực thi trên database
            //không như IQueryable<T>.ForEachAsync() tải bản ghi vào bộ nhớ và thực thi trên từng record
            //sau đó phải gọi DbContext.SaveChangesAsync() mới lưu thay đổi lên database 
            await query.Where(n => n.ToUserId == _currentUser.Id)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));

            _logger.LogInformation("Marking all notifications as read for user: {UserId}", _currentUser.Id);
        }

        //Lỗi This MySqlConnection is already in use là do nhiều truy vấn đang chạy cùng lúc trên 1 MySqlConnection, vốn không hỗ trợ MARS - Multiple Active Result Sets
        //truy vấn trước đó mở hoặc giữ kết nối MySqlConnection và vẫn đang thực thi
        //trong khi MySqlConnection vẫn chưa được giải phóng/câu truy vấn trước đó chưa hoàn thành => nên gây ra lỗi
    }
}