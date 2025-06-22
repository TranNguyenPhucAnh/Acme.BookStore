namespace Acme.BookStore.FeatureManagements;

public static class BookStoreFeatures
{
    public const string GroupName = "BookStore";

    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Enable = Default + ".Enable";
    }

    public static class Authors
    {
        public const string Default = GroupName + ".Authors";
        public const string Enable = Default + ".Enable";
    }
}
