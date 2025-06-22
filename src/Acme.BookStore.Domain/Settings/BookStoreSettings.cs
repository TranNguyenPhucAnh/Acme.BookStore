namespace Acme.BookStore.Settings;

public static class BookStoreSettings
{
    public const string Prefix = "BookStore";
    public const string AbpIdentity = "AbpIdentity";
    public const string RoleProviderName = "R";

    public const string AdminRoleName = "Admin";
    public const string EmployeeRoleName = "Employee";
    public const string ManagerRoleName = "Manager";
    public const string SupportAdminRoleName = "Support Admin";

    public const string ReadOnlyPermission = Prefix + ".Books.";    
    public const string FeatureManagementPermission = "BookStore.FeatureManagement";
    public const string SchedulerPermission = "BookStore.Schedulers";

    //Add your own setting names here. Example:
    //public const string MySetting1 = Prefix + ".MySetting1";
}
