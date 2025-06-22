using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.OrganizationUnits
{
    public class OrganizationUnitAppService(
        IOrganizationUnitRepository organizationUnitRepository,
        IdentityUserManager identityUserManager) : ApplicationService, IOrganizationUnitAppService
    {
        private readonly IOrganizationUnitRepository _organizationUnitRepository = organizationUnitRepository;
        private readonly IdentityUserManager _identityUserManager = identityUserManager;

        public async Task<List<OrganizationUnitEto>> GetOrganizationUnitsAsync()
        {
            return [.. await _organizationUnitRepository.GetListAsync()
                .ContinueWith(x => x.Result.Select(s => new OrganizationUnitEto
                {
                    Id = s.Id,
                    DisplayName = s.DisplayName,
                }))];
        }

        public async Task<List<Guid>> GetUserOrganizationUnitIdsAsync(Guid userId)
        {
            if (userId == Guid.Empty)
            {
                return new List<Guid>();
            }
            var user = await _identityUserManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                throw new ArgumentException($"User with ID {userId} not found.", nameof(userId));
            }
            return [.. await _identityUserManager.GetOrganizationUnitsAsync(user)
                .ContinueWith(x => x.Result.Select(s => s.Id))];
        }
    }
}
