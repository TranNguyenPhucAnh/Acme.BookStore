using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Identity;

namespace Acme.BookStore.Users
{
    public class UserAppService(
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager
        ) : BookStoreAppService, IUserAppService
    {
        private readonly IIdentityUserRepository _identityUserRepository = identityUserRepository;
        private readonly IdentityUserManager _identityUserManager = identityUserManager;

        public async Task<List<IdentityUserDto>> GetAllAsync()
        {
            var users = await _identityUserRepository.GetListAsync();
            return [.. users.Select(user => new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
            })];
        }

        public async Task<List<IdentityUserDto>> GetUsersAsync(List<Guid> ids)
        {
            var users = await _identityUserRepository.GetListByIdsAsync(ids);
            return [.. users.Select(user => new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
            })];
        }

        public async Task<List<IdentityUserDto>> GetUsersByRolesAsync(Guid roleId) //to do fix it to receive a list of role ids
        {
            var userIds = await _identityUserRepository.GetUserIdListByRoleIdAsync(roleId);
            var users = await _identityUserRepository.GetListByIdsAsync(userIds);
            return [.. users.Select(user => new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
            })];
        }

        public async Task<List<IdentityUserDto>> GetUsersByNormalizedRoleNameAsync(string normalizedRoleName)
        {
            return [.. (await _identityUserRepository.GetListByNormalizedRoleNameAsync(normalizedRoleName))
                .Select(user => new IdentityUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    Name = user.Name,
                    Surname = user.Surname,
                })];
        }

        public async Task<List<IdentityUserDto>> GetUsersInOrganizationUnitsAsync(List<Guid> ids)
        {
            var users = await _identityUserRepository.GetUsersInOrganizationsListAsync(ids);

            return [.. users.Select(user => new IdentityUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Surname = user.Surname,
            })];
        }
    }
}
