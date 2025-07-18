namespace Acme.BookStore.Commons
{
    public class ImportErrorDto
    {
        public string Message { get; set; }
        public string Column { get; set; }
        public int Row { get; set; }
    }
}
