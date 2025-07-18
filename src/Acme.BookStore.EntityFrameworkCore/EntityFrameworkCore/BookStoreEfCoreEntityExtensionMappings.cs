using System;
using Volo.Abp.Identity;
using Volo.Abp.ObjectExtending;
using Volo.Abp.Threading;

namespace Acme.BookStore.EntityFrameworkCore;

public static class BookStoreEfCoreEntityExtensionMappings
{
    private static readonly OneTimeRunner OneTimeRunner = new OneTimeRunner();

    public static void Configure()
    {
        BookStoreGlobalFeatureConfigurator.Configure();
        BookStoreModuleExtensionConfigurator.Configure();

        //this class is used to configure EF Core specific entity extension mappings
        OneTimeRunner.Run(() =>
        {
            ObjectExtensionManager.Instance
                .MapEfCoreProperty<IdentityUser, Guid?>("EntityId",
                    (entityBuilder, propertyBuilder) =>
                    {
                        propertyBuilder.IsRequired(false); // Foreign key có thể null
                    });
        });
    }
}
