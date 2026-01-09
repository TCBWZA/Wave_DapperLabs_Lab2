using Bogus;
using DapperLabs_Lab2.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace DapperLabs_Lab2
{
    public static class BogusDataGenerator
    {
        public static async Task SeedDatabase(
            string connectionString,
            int customerCount = 50,
            int minInvoicesPerCustomer = 1,
            int maxInvoicesPerCustomer = 5,
            int minPhoneNumbersPerCustomer = 1,
            int maxPhoneNumbersPerCustomer = 3)
        {
            using var connection = new SqlConnection(connectionString);
            
            var existingCustomerCount = await connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Customers");
            
            if (existingCustomerCount > 0)
            {
                Console.WriteLine($"Database already contains {existingCustomerCount} customers. Skipping seed.");
                return;
            }

            Console.WriteLine($"Database is empty. Starting seed process...");
            Console.WriteLine($"Generating {customerCount} customers with invoices and phone numbers...");

            var random = new Random();

            var customers = SeedCustomer(customerCount);
            
            foreach (var customer in customers)
            {
                // [DAPPER] INSERT with OUTPUT clause - Get auto-generated ID
                var sql = @"
                    INSERT INTO Customers (Name, Email) 
                    OUTPUT INSERTED.Id
                    VALUES (@Name, @Email)";
                
                customer.Id = await connection.ExecuteScalarAsync<long>(sql, customer);
            }

            Console.WriteLine($"? Created {customers.Count} customers");

            var allInvoices = new List<Invoice>();
            var allPhoneNumbers = new List<TelephoneNumber>();

            foreach (var customer in customers)
            {
                int invoiceCount = random.Next(minInvoicesPerCustomer, maxInvoicesPerCustomer + 1);
                var invoices = GenerateInvoices(customer.Id, invoiceCount);
                allInvoices.AddRange(invoices);

                int phoneCount = random.Next(minPhoneNumbersPerCustomer, maxPhoneNumbersPerCustomer + 1);
                var phoneNumbers = GeneratePhoneNumbers(customer.Id, phoneCount);
                allPhoneNumbers.AddRange(phoneNumbers);
            }

            Console.WriteLine($"? Generated {allInvoices.Count} invoices");
            Console.WriteLine($"? Generated {allPhoneNumbers.Count} phone numbers");

            foreach (var invoice in allInvoices)
            {
                var sql = @"
                    INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, Amount) 
                    VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @Amount)";
                
                await connection.ExecuteAsync(sql, invoice);
            }

            foreach (var phone in allPhoneNumbers)
            {
                var sql = @"
                    INSERT INTO TelephoneNumbers (CustomerId, Type, Number) 
                    VALUES (@CustomerId, @Type, @Number)";
                
                await connection.ExecuteAsync(sql, phone);
            }

            Console.WriteLine($"");
            Console.WriteLine($"? Database seeded successfully!");
            Console.WriteLine($"  Total: {customers.Count} customers, {allInvoices.Count} invoices, {allPhoneNumbers.Count} phone numbers");
        }

        public static List<Customer> SeedCustomer(int count)
        {
            Faker<Customer> faker = new Faker<Customer>("en_GB")
                .CustomInstantiator(f => new Customer
                {
                    Name = f.Company.CompanyName(),
                    Email = f.Internet.Email()
                });

            List<Customer> list = [];
            for (int i = 0; i < count; i++)
            {
                list.Add(faker.Generate());
            }

            return list;
        }

        public static List<Invoice> GenerateInvoices(long custId, int count)
        {
            Faker<Invoice> faker = new Faker<Invoice>("en_GB")
                .CustomInstantiator(f =>
                {
                    string invoiceNumber = "INV-" + f.Random.AlphaNumeric(8).ToUpper();
                    DateTime invoiceDate = f.Date.Past(2);
                    decimal amount = f.Finance.Amount(10, 5000);

                    Invoice? invoice = new()
                    {
                        InvoiceNumber = invoiceNumber,
                        InvoiceDate = invoiceDate,
                        Amount = amount,
                        CustomerId = custId
                    };

                    return invoice;
                });

            List<Invoice> list = [];
            for (int i = 0; i < count; i++)
            {
                list.Add(faker.Generate());
            }

            return list;
        }

        private static readonly string[] NumberTypes = new[] { "DirectDial", "Work", "Mobile" };

        public static List<TelephoneNumber> GeneratePhoneNumbers(long custId, int count)
        {
            Faker<TelephoneNumber> fakerphone = new Faker<TelephoneNumber>("en_GB")
                .CustomInstantiator(f => new TelephoneNumber
                {
                    CustomerId = custId,
                    Number = f.Phone.PhoneNumber(),
                    Type = f.PickRandom(NumberTypes)
                });
            
            List<TelephoneNumber> listPhone = [];
            for (int i = 0; i < count; i++)
            {
                listPhone.Add(fakerphone.Generate());
            }
            
            return listPhone;
        }
    }
}
