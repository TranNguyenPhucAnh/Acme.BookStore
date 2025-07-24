using Acme.BookStore.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;

namespace Acme.BookStore.FeatureManagements
{
    //[Authorize(BookStorePermissions.FeatureManagement.Default)]
    public class FeatureManagementAppService(
        IFeatureValueRepository featureValueRepository,
        IFeatureDefinitionManager featureDefinitionManager,
        IFeatureChecker featureChecker,
        IFeatureManager featureManager
        ) : BookStoreAppService, IFeatureManagementAppService
    {

        private readonly IFeatureValueRepository _featureValueRepository = featureValueRepository;
        private readonly IFeatureDefinitionManager _featureDefinitionManager = featureDefinitionManager;
        private readonly IFeatureChecker _featureChecker = featureChecker;
        private readonly IFeatureManager _featureManager = featureManager;

        public async Task CreateFeatureValuesAsync(List<FeatureProviderDto> input)
        {
            if (!(await _featureChecker.IsEnabledAsync(BookStoreFeatures.Books.Enable))
                && !(await _featureChecker.IsEnabledAsync(BookStoreFeatures.Authors.Enable)))
            {
                throw new UserFriendlyException("YouDoNotHaveAccessToFeatureManagement", "400");
            }
            if (input == null)
            {
                throw new ArgumentException("Input list cannot be null.", nameof(input));
            }

            var featureQuery = await _featureValueRepository.GetListAsync();

            await _featureValueRepository.DeleteManyAsync(featureQuery);

            foreach (var feature in input)
            {
                if (feature.Name == null || string.IsNullOrWhiteSpace(feature.Name) || feature.Key == null || string.IsNullOrWhiteSpace(feature.Key))
                {
                    continue; // Skip invalid features
                }
                await _featureManager.SetAsync(feature.Name, true.ToString(), OrganizationFeatureValueProvider.ProviderName, feature.Key);
            }
        }

        public async Task<List<FeatureProviderDto>> GetFeatureValuesAsync()
        {
            var list = await _featureValueRepository.GetListAsync();
            return [.. list
                .Select(x => new FeatureProviderDto
                {
                    Name = x.Name,
                    Key = x.ProviderKey
                })];
        }   

        public async Task<IEnumerable<FeatureDefinition>> GetFeatureDefinitionsAsync()
        {
            return (await _featureDefinitionManager.GetAllAsync()).Where(x => x.Name.Contains(BookStoreFeatures.GroupName) && x.DefaultValue == "true");
        }
    }
}
