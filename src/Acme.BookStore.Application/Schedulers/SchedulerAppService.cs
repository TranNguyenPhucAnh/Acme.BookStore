using Acme.BookStore.Permissions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using CronExpressionDescriptor;
using System.Collections.Generic;

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

        public override Task<SchedulerDto> CreateAsync(CreateUpdateSchedulerDto input)
        {
            input.Description = ExpressionDescriptor.GetDescription(input.CronExpression);
            
            return base.CreateAsync(input);
        }


        public override Task<SchedulerDto> UpdateAsync(Guid id, CreateUpdateSchedulerDto input)
        {
            input.Description = ExpressionDescriptor.GetDescription(input.CronExpression);

            return base.UpdateAsync(id, input);
        }
        public async Task<ListResultDto<SchedulerDto>> GetAllAsync()
        {
            return await Repository.GetListAsync().ContinueWith(task =>
            {
                return new ListResultDto<SchedulerDto>(
                    ObjectMapper.Map<List<Scheduler>, List<SchedulerDto>>(task.Result));
            });
        }
    }
}