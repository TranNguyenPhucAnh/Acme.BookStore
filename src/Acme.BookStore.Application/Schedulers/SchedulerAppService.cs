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

            //also, handle 302 request reached the end of the middleware pipeline without being handled by application code
            return base.CreateAsync(input);
        }

        public override Task<SchedulerDto> UpdateAsync(Guid id, CreateUpdateSchedulerDto input)
        {
            input.CronExpression = ConvertLocalCronToUtcCron(input.CronExpression, input.TimeZone);

            return base.UpdateAsync(id, input);
        }

        private static string ConvertLocalCronToUtcCron(string localCronExpression, string localTimeZoneId)
        {
            // Kiểm tra tính hợp lệ của biểu thức cron
            var cron = CronExpression.Parse(localCronExpression, CronFormat.Standard);
            var parts = localCronExpression.Split(' ');
            if (parts.Length < 5)
                throw new ArgumentException("Invalid cron expression");

            // Lấy múi giờ local
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

            // Tính thời điểm xảy ra tiếp theo trong múi giờ local
            var now = DateTime.Now;
            var nextOccurrenceLocal = cron.GetNextOccurrence(now, localTimeZone);
            if (!nextOccurrenceLocal.HasValue)
                throw new InvalidOperationException("Cannot determine next occurrence");

            // Chuyển đổi sang UTC
            var nextOccurrenceUtc = TimeZoneInfo.ConvertTimeToUtc(nextOccurrenceLocal.Value, localTimeZone);

            // Tạo biểu thức cron mới cho UTC
            var utcHour = nextOccurrenceUtc.Hour;
            var utcMinute = nextOccurrenceUtc.Minute;
            var utcDayOfWeek = ((int)nextOccurrenceUtc.DayOfWeek + 6) % 7; // Chuyển sang định dạng cron (0=Chủ nhật)

            // Cập nhật giờ, phút và thứ
            parts[0] = utcMinute.ToString(); // Phút
            parts[1] = utcHour.ToString();   // Giờ
            if (parts[4] != "*") // Chỉ cập nhật nếu trường thứ không phải "*"
            {
                // Điều chỉnh trường thứ dựa trên chênh lệch ngày
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