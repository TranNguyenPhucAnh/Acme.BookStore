namespace Acme.BookStore.Notifications
{
    public interface INotifiableEntity
    {
        object[] GetLocalizationNotificationArgs(string crudAction);
        object[] GetLocalizationSubjectArgs(string crudAction);
        object[] GetLocalizationBodyArgs(string crudAction);
        string GetLocalizationKey(string keyType, NotificationType type);
    }
}
