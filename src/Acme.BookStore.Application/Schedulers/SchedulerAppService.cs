using Acme.BookStore.Permissions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using CronExpressionDescriptor;
using System.Collections.Generic;
using Volo.Abp;

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
            input.Description = ExpressionDescriptor.GetDescription(input.CronExpression);
            //the server is using UTC, handle timezone locally here
            //also, handle 302 request reached the end of the middleware pipeline without being handled by application code
            return base.CreateAsync(input);
        }

        public override Task<SchedulerDto> UpdateAsync(Guid id, CreateUpdateSchedulerDto input)
        {
            input.Description = ExpressionDescriptor.GetDescription(input.CronExpression);

            return base.UpdateAsync(id, input);
        }
    }
}