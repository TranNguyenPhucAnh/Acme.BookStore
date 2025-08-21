using Volo.Abp.DependencyInjection;
using Volo.Abp.Emailing.Localization;
using Volo.Abp.Emailing.Templates;
using Volo.Abp.Localization;
using Volo.Abp.TextTemplating;
using Volo.Abp.TextTemplating.Scriban;

namespace Acme.BookStore.Emails;

public class CustomEmailTemplateDefinitionProvider : TemplateDefinitionProvider, ITransientDependency
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                StandardEmailTemplates.Layout,
                displayName: LocalizableString.Create<EmailingResource>("TextTemplate:StandardEmailTemplates.Layout"),
                isLayout: true
            ).WithVirtualFilePath("Emails/Templates/Emailing/Layout.tpl", true)
             .WithScribanEngine()
        );

        context.Add(
            new TemplateDefinition(
                StandardEmailTemplates.Message,
                displayName: LocalizableString.Create<EmailingResource>("TextTemplate:StandardEmailTemplates.Message"),
                layout: StandardEmailTemplates.Layout
            ).WithVirtualFilePath("Emails/Templates/Emailing/Message.tpl", true)
            .WithScribanEngine()
        );
    }
}