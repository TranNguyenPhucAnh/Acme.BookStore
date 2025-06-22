using Acme.BookStore.OrganizationUnits;
using System;
using System.Threading.Tasks;
using Volo.Abp.Features;
using Volo.Abp.Users;
using Volo.Abp.Validation.StringValues;

namespace Acme.BookStore.FeatureManagements
{
    public class OrganizationFeatureValueProvider(
        IFeatureStore featureStore,
        ICurrentUser currentUser,
        IOrganizationUnitAppService organizationUnitAppService) : FeatureValueProvider(featureStore)
    {
        public const string ProviderName = "O";
        public override string Name => ProviderName;

        private readonly IOrganizationUnitAppService _organizationUnitAppService = organizationUnitAppService;
        private readonly ICurrentUser _currentUser = currentUser;

        public override async Task<string> GetOrNullAsync(FeatureDefinition feature)
        {
            if (feature.ValueType is ToggleStringValueType && feature.Name.StartsWith(BookStoreFeatures.GroupName))
            {
                var userId = _currentUser.Id.GetValueOrDefault();

                if (userId == Guid.Empty)
                {
                    return "false"; 
                }

                var userOrgIds = await _organizationUnitAppService.GetUserOrganizationUnitIdsAsync(userId);
                foreach (var orgUnitId in userOrgIds)
                {
                    var value = await FeatureStore.GetOrNullAsync(feature.Name, Name, orgUnitId.ToString());
                    if (value != null && value.ToLower() == "true")
                    {
                        return "true";
                    }
                }
                return null;
            }
            return null;
        }
    }
}
