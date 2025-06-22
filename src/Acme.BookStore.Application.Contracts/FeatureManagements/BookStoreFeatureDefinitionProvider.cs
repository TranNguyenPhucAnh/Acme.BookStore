using Volo.Abp.Features;

namespace Acme.BookStore.FeatureManagements
{
    public class BookStoreFeatureDefinitionProvider : FeatureDefinitionProvider
    {
        public override void Define(IFeatureDefinitionContext context)
        {
            var myGroup = context.AddGroup(BookStoreFeatures.GroupName);

            myGroup.AddFeature(BookStoreFeatures.Books.Enable, defaultValue: "true");

            myGroup.AddFeature(BookStoreFeatures.Authors.Enable, defaultValue: "true");
        }
    }
} 