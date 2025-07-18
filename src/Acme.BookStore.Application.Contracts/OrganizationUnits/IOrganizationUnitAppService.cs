using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Identity;

namespace Acme.BookStore.OrganizationUnits
{
    public interface IOrganizationUnitAppService : IApplicationService
    {
        Task<List<OrganizationUnitEto>> GetAllAsync();
        Task<List<Guid>> GetUserOrganizationUnitIdsAsync(Guid userId); // to do: fix it to receive a list of user IDs
    }
}
