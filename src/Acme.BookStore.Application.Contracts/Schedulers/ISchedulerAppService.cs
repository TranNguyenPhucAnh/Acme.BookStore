using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Acme.BookStore.Schedulers
{
    public interface ISchedulerAppService :
        ICrudAppService<
        SchedulerDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateSchedulerDto>
    {
        Task<ListResultDto<SchedulerDto>> GetAllAsync();
    }
}
