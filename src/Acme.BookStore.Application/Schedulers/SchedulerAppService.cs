using Acme.BookStore.Permissions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Collections.Generic;
using Volo.Abp;
using Cronos;

namespace Acme.BookStore.Schedulers
{
    public class SchedulerAppService
        : CrudAppService<
            Scheduler,
            SchedulerDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateSchedulerDto>,
        ISchedulerAppService
    {
        public SchedulerAppService(IRepository<Scheduler, Guid> repository) : base(repository)
        {
            GetPolicyName = BookStorePermissions.Schedulers.Default;
            GetListPolicyName = BookStorePermissions.Schedulers.Default;
            CreatePolicyName = BookStorePermissions.Schedulers.Create;
            UpdatePolicyName = BookStorePermissions.Schedulers.Edit;
            DeletePolicyName = BookStorePermissions.Schedulers.Delete;
        }

        [RemoteService(IsEnabled = false)]
        public async Task<ListResultDto<SchedulerDto>> GetAllAsync()
        {
            var list = await Repository.GetListAsync();
            return new ListResultDto<SchedulerDto>(
                ObjectMapper.Map<List<Scheduler>, List<SchedulerDto>>(list));
        }

        public override Task<SchedulerDto> CreateAsync(CreateUpdateSchedulerDto input)
        {
            input.CronExpression = ConvertLocalCronToUtcCron(input.CronExpression, input.TimeZone);

            return base.CreateAsync(input);
        }

        public override Task<SchedulerDto> UpdateAsync(Guid id, CreateUpdateSchedulerDto input)
        {
            input.CronExpression = ConvertLocalCronToUtcCron(input.CronExpression, input.TimeZone);

            return base.UpdateAsync(id, input);
        }

        public static string ConvertLocalCronToUtcCron(string localCronExpression, string localTimeZoneId)
        {
            // Kiểm tra tính hợp lệ của biểu thức cron
            var cron = CronExpression.Parse(localCronExpression, CronFormat.Standard);
            var parts = localCronExpression.Split(' ');
            if (parts.Length < 5)
                throw new ArgumentException("Invalid cron expression");

            // Lấy múi giờ địa phương
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

            // Tạo thời điểm tham chiếu (00:00 hôm nay) trong múi giờ địa phương
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, localTimeZone);
            var referenceTime = new DateTime(nowLocal.Year, nowLocal.Month, nowLocal.Day, 0, 0, 0, DateTimeKind.Unspecified);

            // Tính thời điểm xảy ra tiếp theo trong múi giờ địa phương
            var nextOccurrenceLocal = cron.GetNextOccurrence(referenceTime, localTimeZone);
            if (!nextOccurrenceLocal.HasValue)
                throw new InvalidOperationException("Cannot determine next occurrence");

            // Chuyển đổi sang UTC
            var localTime = DateTime.SpecifyKind(nextOccurrenceLocal.Value, DateTimeKind.Unspecified);
            var nextOccurrenceUtc = TimeZoneInfo.ConvertTimeToUtc(localTime, localTimeZone);

            // Cập nhật phút, giờ và thứ (nếu cần)
            parts[0] = nextOccurrenceUtc.Minute.ToString(); // Phút
            parts[1] = nextOccurrenceUtc.Hour.ToString();   // Giờ
            if (parts[4] != "*") // Chỉ cập nhật nếu trường thứ không phải "*"
            {
                var utcDayOfWeek = ((int)nextOccurrenceUtc.DayOfWeek + 6) % 7; // Định dạng cron (0=Chủ nhật)
                var localDayOfWeek = ((int)nextOccurrenceLocal.Value.DayOfWeek + 6) % 7;
                var dayOfWeekShift = (utcDayOfWeek - localDayOfWeek + 7) % 7;
                if (int.TryParse(parts[4], out var originalDayOfWeek))
                {
                    var newDayOfWeek = (originalDayOfWeek + dayOfWeekShift) % 7;
                    parts[4] = newDayOfWeek.ToString();
                }
                // Lưu ý: Với các biểu thức phức tạp (như 1-5, */2), cần xử lý thêm
            }

            return string.Join(" ", parts);
        }
    }
}