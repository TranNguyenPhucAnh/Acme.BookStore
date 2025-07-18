using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.Roles
{
    public interface IRoleAppService : IApplicationService
    {
        Task<List<IdentityRoleDto>> GetAllAsync();
    }
}
