using Acme.BookStore.Notifications;
using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Books;

public class BookDto : AuditedEntityDto<Guid>, INotifiableEntity
{
    public Guid AuthorId { get; set; }

    public string ISBN { get; set; } = default!;

    public string AuthorName { get; set; } = default!;

    public string Name { get; set; } = default!;

    public BookType Type { get; set; }

    public DateTime PublishDate { get; set; }

    public string Publisher { get; set; } = default!;

    public string GetLocalizationKey(string keyType, NotificationType type)
    {
        return keyType switch
        {
            "Subject" => type == NotificationType.BookCRUD ? "Email:Subject:BookCRUD" : "Email:Subject:Fallback",
            "Body" => type == NotificationType.BookCRUD ? "Email:Body:BookCRUD" : "Email:Body:Fallback",
            //render from BE does not need double colon, but render from FE need double colon
            "Notification" => type == NotificationType.BookCRUD ? "::Notification:BookCRUD" : "::Notification:Fallback",
            _ => throw new ArgumentException($"Unknown key type: {keyType}", nameof(keyType)),
        };
    }

    public object[] GetLocalizationNotificationArgs(string crudAction)
    {
        return
        [
            crudAction,
            Name,
            AuthorName,
            Type.ToString(),
            Publisher,
            PublishDate.ToString("d MMMM yyyy")
        ];
    }

    public object[] GetLocalizationBodyArgs(string crudAction)
    {
        return
        [
            crudAction,
            Name,
            AuthorName,
            Type.ToString(),
            Publisher,
            PublishDate.ToString("d MMMM yyyy")
        ];
    }

    public object[] GetLocalizationSubjectArgs(string crudAction)
    {
        return
        [
           Name,
           crudAction,
        ];
    }
}
