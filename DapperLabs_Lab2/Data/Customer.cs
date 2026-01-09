namespace DapperLabs_Lab2.Data
{
    public class Customer
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public List<Invoice>? Invoices { get; set; } = new List<Invoice>();
        public decimal Balance => Invoices?.Sum(i => i.Amount) ?? 0;
        public List<TelephoneNumber>? PhoneNumbers { get; set; } = new List<TelephoneNumber>();
    }
}
