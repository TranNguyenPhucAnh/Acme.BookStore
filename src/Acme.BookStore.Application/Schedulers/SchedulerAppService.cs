using Acme.BookStore.Permissions;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
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
            //ListResultDto instead of PagedResultDto avoid handling pagination
            var list = await Repository.GetListAsync();
            return new ListResultDto<SchedulerDto>(
                ObjectMapper.Map<List<Scheduler>, List<SchedulerDto>>(list));
        }
    }
}