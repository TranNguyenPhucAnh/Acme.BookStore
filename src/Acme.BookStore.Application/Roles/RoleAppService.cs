using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace Acme.BookStore.Roles
{
    public class RoleAppService(IIdentityRoleRepository identityRoleRepository)
        : BookStoreAppService, IRoleAppService
    {
        private readonly IIdentityRoleRepository _identityRoleRepository = identityRoleRepository;

        public async Task<List<IdentityRoleDto>> GetAllAsync()
        {
            return await _identityRoleRepository.GetListAsync()
                .ContinueWith(task => task.Result.Select(role => new IdentityRoleDto
                {
                    Id = role.Id,
                    Name = role.Name
                }).ToList());
        }
    }
}
