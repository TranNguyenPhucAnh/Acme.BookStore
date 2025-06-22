using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;

namespace Acme.BookStore.FeatureManagements
{
    public interface IFeatureManagementAppService : IApplicationService
    {
        Task CreateFeatureValuesAsync(List<FeatureProviderDto> input);
        Task<List<FeatureProviderDto>> GetFeatureValuesAsync();
        Task<IEnumerable<FeatureDefinition>> GetFeatureDefinitionsAsync();
    }
}
