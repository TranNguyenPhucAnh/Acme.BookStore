using Acme.BookStore.FeatureManagements;
using Acme.BookStore.OrganizationUnits;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations;
using Volo.Abp.AspNetCore.Mvc.ApplicationConfigurations.ObjectExtending;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Users;

namespace Acme.BookStore.Controllers.ApplicationConfigurations
{
    [ExposeServices(typeof(IAbpApplicationConfigurationAppService), typeof(AbpApplicationConfigurationAppService))]
    public class BookStoreApplicationConfiguration(
        IOptions<AbpLocalizationOptions> localizationOptions,
        IOptions<AbpMultiTenancyOptions> multiTenancyOptions,
        IServiceProvider serviceProvider,
        IAbpAuthorizationPolicyProvider abpAuthorizationPolicyProvider,
        IPermissionDefinitionManager permissionDefinitionManager,
        DefaultAuthorizationPolicyProvider defaultAuthorizationPolicyProvider,
        IPermissionChecker permissionChecker,
        IAuthorizationService authorizationService,
        ICurrentUser currentUser,
        ISettingProvider settingProvider,
        ISettingDefinitionManager settingDefinitionManager,
        IFeatureDefinitionManager featureDefinitionManager,
        ILanguageProvider languageProvider,
        ITimezoneProvider timezoneProvider,
        IOptions<AbpClockOptions> abpClockOptions,
        ICachedObjectExtensionsDtoService cachedObjectExtensionsDtoService,
        IOptions<AbpApplicationConfigurationOptions> options,
        IFeatureManagementAppService featureManagementAppService,
        IOrganizationUnitAppService organizationUnitAppService
        )
        : AbpApplicationConfigurationAppService(
            localizationOptions,
            multiTenancyOptions,
            serviceProvider,
            abpAuthorizationPolicyProvider,
            permissionDefinitionManager,
            defaultAuthorizationPolicyProvider,
            permissionChecker,
            authorizationService,
            currentUser,
            settingProvider,
            settingDefinitionManager,
            featureDefinitionManager,
            languageProvider,
            timezoneProvider,
            abpClockOptions,
            cachedObjectExtensionsDtoService,
            options)
        
    {
        private readonly IFeatureManagementAppService _featureManagementAppService = featureManagementAppService;
        private readonly IOrganizationUnitAppService _organizationUnitAppService = organizationUnitAppService;

        public override async Task<ApplicationConfigurationDto> GetAsync(ApplicationConfigurationRequestOptions options)
        {
            var configuration = await base.GetAsync(options);

            var features = await _featureManagementAppService.GetFeatureValuesAsync();

            var userOrgIds = await _organizationUnitAppService.GetUserOrganizationUnitIdsAsync(CurrentUser.Id.GetValueOrDefault());

            configuration.ExtraProperties["EnabledFeatures"] = features.Where(f => userOrgIds.Contains(Guid.Parse(f.Key))).Select(s => s.Name);

            return configuration;
        }
    }
}
