namespace Acme.BookStore.Notifications
{
    public class EmailTemplateModel
    {
        public string DistributionDomainName { get; set; }  
        public int Year { get; set; }
        public string LocalizedMessage { get; set; }
    }
}
