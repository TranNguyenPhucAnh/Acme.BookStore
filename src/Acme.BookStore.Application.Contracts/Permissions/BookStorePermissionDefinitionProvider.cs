using Acme.BookStore.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;

namespace Acme.BookStore.Permissions;

public class BookStorePermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var bookStoreGroup = context.AddGroup(BookStorePermissions.GroupName, L("Permission:BookStore"));

        var booksPermission = bookStoreGroup.AddPermission(BookStorePermissions.Books.Default, L("Permission:Books"));
        booksPermission.AddChild(BookStorePermissions.Books.Create, L("Permission:Books.Create"));
        booksPermission.AddChild(BookStorePermissions.Books.Edit, L("Permission:Books.Edit"));
        booksPermission.AddChild(BookStorePermissions.Books.Delete, L("Permission:Books.Delete"));

        var authorsPermission = bookStoreGroup.AddPermission(BookStorePermissions.Authors.Default, L("Permission:Authors"));
        authorsPermission.AddChild(BookStorePermissions.Authors.Create, L("Permission:Authors.Create"));
        authorsPermission.AddChild(BookStorePermissions.Authors.Edit, L("Permission:Authors.Edit"));
        authorsPermission.AddChild(BookStorePermissions.Authors.Delete, L("Permission:Authors.Delete"));

        var schedulersPermission = bookStoreGroup.AddPermission(BookStorePermissions.Schedulers.Default, L("Permission:Schedulers"));
        schedulersPermission.AddChild(BookStorePermissions.Schedulers.Create, L("Permission:Schedulers.Create"));
        schedulersPermission.AddChild(BookStorePermissions.Schedulers.Edit, L("Permission:Schedulers.Edit"));
        schedulersPermission.AddChild(BookStorePermissions.Schedulers.Delete, L("Permission:Schedulers.Delete"));

        var featurePermission = bookStoreGroup.AddPermission(BookStorePermissions.FeatureManagement.Default, L("Permission:FeatureManagement"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<BookStoreResource>(name);
    }
}
