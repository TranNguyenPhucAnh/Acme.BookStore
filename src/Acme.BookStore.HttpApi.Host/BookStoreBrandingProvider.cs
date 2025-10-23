using Microsoft.Extensions.Localization;
using Acme.BookStore.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace Acme.BookStore;

[Dependency(ReplaceServices = true)]
public class BookStoreBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<BookStoreResource> _localizer;

    public BookStoreBrandingProvider(IStringLocalizer<BookStoreResource> localizer)
    {
        _localizer = localizer;
    }

    //change document title in BrandingProvider & localization files added "AppName" key
    public override string AppName => "Phuc Anh's Demo Project"; //_localizer["Anh's Demo App"];
}
