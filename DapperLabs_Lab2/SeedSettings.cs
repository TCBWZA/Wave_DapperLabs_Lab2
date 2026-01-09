namespace DapperLabs_Lab2
{
    public class SeedSettings
    {
        public bool EnableSeeding { get; set; }
        public int CustomerCount { get; set; } = 50;
        public int MinInvoicesPerCustomer { get; set; } = 1;
        public int MaxInvoicesPerCustomer { get; set; } = 5;
        public int MinPhoneNumbersPerCustomer { get; set; } = 1;
        public int MaxPhoneNumbersPerCustomer { get; set; } = 3;
    }
}
