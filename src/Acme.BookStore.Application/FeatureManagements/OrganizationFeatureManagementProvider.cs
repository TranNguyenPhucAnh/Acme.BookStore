using Volo.Abp.DependencyInjection;
using Volo.Abp.FeatureManagement;

namespace Acme.BookStore.FeatureManagements
{
    public class OrganizationFeatureManagementProvider(IFeatureManagementStore store) : FeatureManagementProvider(store), ITransientDependency
    {
        public override string Name => OrganizationFeatureValueProvider.ProviderName;
    }
}
