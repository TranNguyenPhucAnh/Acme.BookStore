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
            var parts = localCronExpression.Split(' ');
            if (parts.Length < 5)
                throw new ArgumentException("Invalid cron expression");

            // Lấy timezone
            var localTimeZone = TimeZoneInfo.FindSystemTimeZoneById(localTimeZoneId);

            // Lấy offset hiện tại
            var now = DateTime.UtcNow;
            var offset = localTimeZone.GetUtcOffset(now);

            // Giả sử cron chỉ có minute hour day month dayOfWeek
            if (!int.TryParse(parts[0], out var minute) ||
                !int.TryParse(parts[1], out var hour))
            {
                throw new NotSupportedException("Chỉ hỗ trợ cron số đơn giản (không có */5, 1-5)");
            }

            // Tạo thời gian local mẫu
            var localSample = new DateTime(2025, 1, 6, hour, minute, 0, DateTimeKind.Unspecified);
            var localWithKind = DateTime.SpecifyKind(localSample, DateTimeKind.Unspecified);

            // Chuyển sang UTC
            var utcTime = TimeZoneInfo.ConvertTimeToUtc(localWithKind, localTimeZone);

            // Cập nhật lại cron parts
            parts[0] = utcTime.Minute.ToString();
            parts[1] = utcTime.Hour.ToString();

            return string.Join(" ", parts);
        }
    }
}