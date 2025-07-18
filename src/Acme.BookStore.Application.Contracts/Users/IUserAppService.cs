using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.Users
{
    public interface IUserAppService : IApplicationService
    {
        /// <summary>
        /// Gets a list of users.
        /// </summary>
        /// <returns>A list of user DTOs.</returns>
        Task<List<IdentityUserDto>> GetAllAsync();
        Task<List<IdentityUserDto>> GetUsersAsync(List<Guid> ids);
        Task<List<IdentityUserDto>> GetUsersByRolesAsync(Guid roleId); // to do fix it to receive a list of role ids
        Task<List<IdentityUserDto>> GetUsersInOrganizationUnitsAsync(List<Guid> ids);
        Task<List<IdentityUserDto>> GetUsersByNormalizedRoleNameAsync(string normalizedRoleName);
    }
}
