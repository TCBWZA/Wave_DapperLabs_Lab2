namespace DapperLabs_Lab2.Data
{
    public class Invoice
    {
        public long Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public long CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal Amount { get; set; }
    }
}
