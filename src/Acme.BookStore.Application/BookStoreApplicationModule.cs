using Acme.BookStore.FeatureManagements;
using Acme.BookStore.Users;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Account;
using Volo.Abp.AutoMapper;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.BlobStoring.Aws;
using Volo.Abp.Emailing;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Features;
using Volo.Abp.FluentValidation;
using Volo.Abp.Identity;
using Volo.Abp.Modularity;
using Volo.Abp.PermissionManagement;
using Volo.Abp.SettingManagement;
using Volo.Abp.TextTemplating.Scriban;
using Volo.Abp.VirtualFileSystem;

namespace Acme.BookStore;

[DependsOn(
    typeof(BookStoreDomainModule),
    typeof(BookStoreApplicationContractsModule),
    typeof(AbpPermissionManagementApplicationModule),
    typeof(AbpFeatureManagementApplicationModule),
    typeof(AbpIdentityApplicationModule),
    typeof(AbpAccountApplicationModule),
    typeof(AbpSettingManagementApplicationModule),
    typeof(AbpBackgroundJobsModule),
    typeof(AbpEmailingModule),
    typeof(AbpTextTemplatingScribanModule),
    typeof(AbpFluentValidationModule),
    typeof(AbpBlobStoringAwsModule)
    )]

    public class BookStoreApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Đăng ký VFS cho assembly hiện tại
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<BookStoreApplicationModule>();
        });

        Configure<AbpAutoMapperOptions>(options =>
        {
            options.AddMaps<BookStoreApplicationModule>();
        });

        Configure<FeatureManagementOptions>(options =>
        {
            options.Providers.Add<OrganizationFeatureManagementProvider>();
        });

        Configure<AbpFeatureOptions>(options =>
        {
            options.ValueProviders.Add<OrganizationFeatureValueProvider>();
        });
    }
}
