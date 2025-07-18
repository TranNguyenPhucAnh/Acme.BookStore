using Acme.BookStore.Notifications;
using System;
using Volo.Abp.Application.Dtos;

namespace Acme.BookStore.Authors;

public class AuthorDto : EntityDto<Guid>, INotifiableEntity
{
    public string Name { get; set; }

    public DateTime BirthDate { get; set; }

    public string GetLocalizationKey(string keyType, NotificationType type)
    {
        return keyType switch
        {
            "Subject" => type == NotificationType.AuthorCRUD ? "Email:Subject:AuthorCRUD" : "Email:Subject:Fallback",
            "Body" => type == NotificationType.AuthorCRUD ? "Email:Body:AuthorCRUD" : "Email:Body:Fallback",
            //render from BE does not need double colon, but render from FE need double colon
            "Notification" => type == NotificationType.AuthorCRUD ? "::Notification:AuthorCRUD" : "::Notification:Fallback",
            _ => throw new ArgumentException($"Unknown key type: {keyType}", nameof(keyType)),
        };
    }

    public object[] GetLocalizationNotificationArgs(string crudAction)
    {
        return
        [
            crudAction,
            Name,
            BirthDate.ToString("d MMMM yyyy")
        ];
    }

    public object[] GetLocalizationBodyArgs(string crudAction)
    {
        return
        [
            crudAction,
            Name,
            BirthDate.ToString("d MMMM yyyy")
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
