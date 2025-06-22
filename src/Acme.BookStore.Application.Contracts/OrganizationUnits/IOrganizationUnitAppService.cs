using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.OrganizationUnits
{
    public interface IOrganizationUnitAppService : IApplicationService
    {
        Task<List<OrganizationUnitEto>> GetOrganizationUnitsAsync();
        Task<List<Guid>> GetUserOrganizationUnitIdsAsync(Guid userId);
    }
}
