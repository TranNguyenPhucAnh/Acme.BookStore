using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;

namespace Acme.BookStore.Users
{
    [Dependency(ReplaceServices = true)]
    [ExposeServices(typeof(IIdentityUserAppService), typeof(IdentityUserAppService))]
    public class ExtendedIdentityUserAppService(
        IOrganizationUnitRepository organizationUnitRepository,
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        IIdentityRoleRepository roleRepository,
        IOptions<IdentityOptions> identityOptions,
        IPermissionChecker permissionChecker) :
        IdentityUserAppService(userManager, userRepository, roleRepository, identityOptions, permissionChecker)
    {
        private readonly IOrganizationUnitRepository _organizationUnitRepository = organizationUnitRepository;
        private readonly IdentityUserManager _userManager = userManager;

        public override async Task<IdentityUserDto> CreateAsync(IdentityUserCreateDto input)
        {
            await IdentityOptions.SetAsync();

            var user = new IdentityUser(
                GuidGenerator.Create(),
                input.UserName,
                input.Email,
                CurrentTenant.Id
            );

            var orgId = Guid.Parse(input.ExtraProperties["EntityId"].ToString());
            user.SetProperty("EntityId", orgId);

            var organizationUnit = await _organizationUnitRepository.GetAsync(orgId);
            user.SetProperty("Entity", organizationUnit.DisplayName);

            user.SetProperty("Roles", string.Join(", ", input.RoleNames));

            input.MapExtraPropertiesTo(user);

            (await UserManager.CreateAsync(user, input.Password)).CheckErrors();
            await UpdateUserByInput(user, input);
            (await UserManager.UpdateAsync(user)).CheckErrors();

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
        }

        public override async Task<IdentityUserDto> UpdateAsync(Guid id, IdentityUserUpdateDto input)
        {
            var user = await _userManager.GetByIdAsync(id);
            
            var orgId = Guid.Parse(input.ExtraProperties["EntityId"].ToString());
            user.SetProperty("EntityId", orgId);

            var organizationUnit = await _organizationUnitRepository.GetAsync(orgId);
            user.SetProperty("Entity", organizationUnit.DisplayName);

            user.SetProperty("Roles", string.Join(", ", input.RoleNames));

            await IdentityOptions.SetAsync();

            user.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

            (await UserManager.SetUserNameAsync(user, input.UserName)).CheckErrors();

            await UpdateUserByInput(user, input);

            input.MapExtraPropertiesTo(user);

            (await UserManager.UpdateAsync(user)).CheckErrors();

            if (!input.Password.IsNullOrEmpty())
            {
                (await UserManager.RemovePasswordAsync(user)).CheckErrors();
                (await UserManager.AddPasswordAsync(user, input.Password)).CheckErrors();
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            return ObjectMapper.Map<IdentityUser, IdentityUserDto>(user);
        }
    }
}