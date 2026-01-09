# Dapper Labs API Guide

## Overview

The Dapper Labs API is a RESTful Web API built with ASP.NET Core 8.0 and Dapper ORM. It provides complete CRUD operations for managing customers, invoices, and telephone numbers with built-in Swagger documentation.

## Getting Started

### Prerequisites
- .NET 8 SDK
- SQL Server or SQL Server LocalDB
- A REST client (Postman, curl, or use Swagger UI)

### Running the API

1. **Update connection string** in `appsettings.json`

2. **Restore packages and run:**
   ```powershell
   dotnet restore
   dotnet run
   ```

3. **Access Swagger UI:**
   - Open your browser to: `http://localhost:5000` or `https://localhost:5001`
   - Swagger UI will be displayed at the root URL

## API Endpoints

### Customers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/customers` | Get all customers (paginated) |
| GET | `/api/customers/{id}` | Get customer by ID |
| GET | `/api/customers/search` | Search customers |
| POST | `/api/customers` | Create new customer |
| PUT | `/api/customers/{id}` | Update customer |
| DELETE | `/api/customers/{id}` | Delete customer |

### Invoices

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/invoices` | Get all invoices |
| GET | `/api/invoices/{id}` | Get invoice by ID |
| GET | `/api/invoices/customer/{customerId}` | Get invoices for customer |
| POST | `/api/invoices` | Create new invoice |
| PUT | `/api/invoices/{id}` | Update invoice |
| DELETE | `/api/invoices/{id}` | Delete invoice |

### Telephone Numbers

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/telephonenumbers` | Get all telephone numbers |
| GET | `/api/telephonenumbers/{id}` | Get telephone number by ID |
| GET | `/api/telephonenumbers/customer/{customerId}` | Get phone numbers for customer |
| POST | `/api/telephonenumbers` | Create new telephone number |
| PUT | `/api/telephonenumbers/{id}` | Update telephone number |
| DELETE | `/api/telephonenumbers/{id}` | Delete telephone number |

## API Examples

### 1. Get All Customers (Paginated)

**Request:**
```http
GET /api/customers?page=1&pageSize=10&includeRelated=true
```

**Response:**
```json
{
  "items": [
    {
      "id": 1,
      "name": "Acme Corporation",
      "email": "acme@example.com",
      "balance": 15234.50,
      "invoices": [
        {
          "id": 1,
          "invoiceNumber": "INV-001",
          "customerId": 1,
          "invoiceDate": "2024-01-15T00:00:00",
          "amount": 5234.50
        }
      ],
      "phoneNumbers": [
        {
          "id": 1,
          "customerId": 1,
          "type": "Mobile",
          "number": "+44 7700 900123"
        }
      ]
    }
  ],
  "totalCount": 1000,
  "page": 1,
  "pageSize": 10,
  "totalPages": 100,
  "hasPrevious": false,
  "hasNext": true
}
```

### 2. Search Customers

**Request:**
```http
GET /api/customers/search?name=Acme&minBalance=1000
```

**Response:**
```json
[
  {
    "id": 1,
    "name": "Acme Corporation",
    "email": "acme@example.com",
    "balance": 15234.50,
    "invoices": null,
    "phoneNumbers": null
  }
]
```

### 3. Create a Customer

**Request:**
```http
POST /api/customers
Content-Type: application/json

{
  "name": "New Company Ltd",
  "email": "contact@newcompany.com"
}
```

**Response:**
```json
{
  "id": 1001,
  "name": "New Company Ltd",
  "email": "contact@newcompany.com",
  "balance": 0,
  "invoices": null,
  "phoneNumbers": null
}
```

### 4. Update a Customer

**Request:**
```http
PUT /api/customers/1001
Content-Type: application/json

{
  "name": "New Company Ltd (Updated)",
  "email": "contact@newcompany.com"
}
```

**Response:**
```json
{
  "id": 1001,
  "name": "New Company Ltd (Updated)",
  "email": "contact@newcompany.com",
  "balance": 0,
  "invoices": null,
  "phoneNumbers": null
}
```

### 5. Create an Invoice

**Request:**
```http
POST /api/invoices
Content-Type: application/json

{
  "invoiceNumber": "INV-12345",
  "customerId": 1,
  "invoiceDate": "2024-01-20T00:00:00",
  "amount": 1500.00
}
```

**Response:**
```json
{
  "id": 5001,
  "invoiceNumber": "INV-12345",
  "customerId": 1,
  "invoiceDate": "2024-01-20T00:00:00",
  "amount": 1500.00
}
```

### 6. Get Invoices for a Customer

**Request:**
```http
GET /api/invoices/customer/1
```

**Response:**
```json
[
  {
    "id": 1,
    "invoiceNumber": "INV-001",
    "customerId": 1,
    "invoiceDate": "2024-01-15T00:00:00",
    "amount": 5234.50
  },
  {
    "id": 5001,
    "invoiceNumber": "INV-12345",
    "customerId": 1,
    "invoiceDate": "2024-01-20T00:00:00",
    "amount": 1500.00
  }
}
]
```

### 7. Create a Telephone Number

**Request:**
```http
POST /api/telephonenumbers
Content-Type: application/json

{
  "customerId": 1,
  "type": "Mobile",
  "number": "+44 7700 900456"
}
```

**Response:**
```json
{
  "id": 3001,
  "customerId": 1,
  "type": "Mobile",
  "number": "+44 7700 900456"
}
```

## Using Swagger UI

### Interactive Documentation

1. Navigate to `http://localhost:5000`
2. Browse all available endpoints
3. Click on any endpoint to expand details
4. Click "Try it out" to test the endpoint
5. Fill in parameters and request body
6. Click "Execute" to make the request
7. View the response below

### Features:
- **Schema Definitions**: View data models for requests and responses
- **Authentication**: None required for this demo API
- **Request/Response Examples**: See sample data for each endpoint
- **HTTP Status Codes**: Understand what each response code means

## Error Responses

### 400 Bad Request
Returned when validation fails or business rules are violated.

```json
{
  "message": "Email 'test@example.com' is already in use"
}
```

### 404 Not Found
Returned when a requested resource doesn't exist.

```json
{
  "message": "Customer with ID 9999 not found"
}
```

### 500 Internal Server Error
Returned when an unexpected server error occurs. Check logs for details.

## Validation Rules

### Customer
- **Name**: Required, max 200 characters
- **Email**: Required, valid email format, max 200 characters, must be unique

### Invoice
- **InvoiceNumber**: Required, must start with "INV", max 50 characters, must be unique
- **CustomerId**: Required, must exist
- **InvoiceDate**: Required
- **Amount**: Required, must be >= 0

### Telephone Number
- **CustomerId**: Required, must exist
- **Type**: Required, must be one of: Mobile, Work, DirectDial
- **Number**: Required, max 50 characters

## Query Parameters

### Pagination (Customers endpoint)
- `page` (int, default: 1): Page number
- `pageSize` (int, default: 10, max: 100): Number of items per page
- `includeRelated` (bool, default: false): Include invoices and phone numbers

### Search (Customers search endpoint)
- `name` (string, optional): Partial match on customer name
- `email` (string, optional): Partial match on customer email
- `minBalance` (decimal, optional): Minimum balance filter

## Logging

The API includes comprehensive logging:

- **Request logging**: All API requests are logged with parameters
- **Error logging**: Errors include stack traces and context
- **Information logging**: Database operations and important events

View logs in the console output when running the application.

### Log Levels
- **Information**: Normal operations
- **Warning**: Potential issues (e.g., not found)
- **Error**: Exceptions and failures

## CORS Configuration

The API is configured with a permissive CORS policy for development:
- Allows all origins
- Allows all methods
- Allows all headers

**Note**: Update the CORS policy for production environments to restrict access appropriately.

## Configuration

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "DapperLabs_Lab2": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DapperLabs;..."
  },
  "SeedSettings": {
    "EnableSeeding": true,
    "CustomerCount": 1000
  }
}
```

### Key Settings:
- **Logging**: Configure log levels per namespace
- **ConnectionStrings**: Database connection string
- **SeedSettings**: Control automatic data seeding

## Testing the API

### Using curl (Windows PowerShell)

**Get all customers:**
```powershell
curl http://localhost:5000/api/customers
```

**Create a customer:**
```powershell
curl -X POST http://localhost:5000/api/customers `
  -H "Content-Type: application/json" `
  -d '{"name":"Test Corp","email":"test@test.com"}'
```

**Update a customer:**
```powershell
curl -X PUT http://localhost:5000/api/customers/1 `
  -H "Content-Type: application/json" `
  -d '{"name":"Updated Corp","email":"updated@test.com"}'
```

**Delete a customer:**
```powershell
curl -X DELETE http://localhost:5000/api/customers/1
```

### Using Postman

1. Import the API by using the Swagger JSON
2. Create a new collection
3. Add requests for each endpoint
4. Save and organize your requests
5. Create environments for different configurations

## Performance Considerations

### Pagination
- Always use pagination for large datasets
- Default page size is 10, maximum is 100
- Use `includeRelated=false` when you don't need related data

### Lazy Loading
- Related data (invoices, phone numbers) is only loaded when `includeRelated=true`
- This reduces payload size and improves performance

### Database Optimization
- All queries use parameterized SQL (SQL injection safe)
- Indexes on Email (Customers) and InvoiceNumber (Invoices)
- Foreign keys with CASCADE DELETE for data integrity

## Best Practices

### API Clients
1. Always handle error responses appropriately
2. Use pagination for list endpoints
3. Validate data before sending requests
4. Cache responses when appropriate
5. Use async/await for better performance

### Data Management
1. Check for existence before creating (email, invoice number)
2. Use transactions for related operations
3. Validate foreign key relationships
4. Handle concurrent updates appropriately

## Troubleshooting

### API won't start
- Check SQL Server is running
- Verify connection string in appsettings.json
- Check port 5000/5001 is not in use

### Swagger UI not loading
- Ensure you're in Development environment
- Check `ASPNETCORE_ENVIRONMENT` is set to "Development"
- Verify Swagger is configured in Program.cs

### Requests failing
- Check request body matches DTO structure
- Verify all required fields are provided
- Check validation rules are met
- Review logs for detailed error messages

## Next Steps

1. **Explore Swagger UI**: Test all endpoints interactively
2. **Create Test Data**: Use POST endpoints to create customers, invoices, and phone numbers
3. **Test Relationships**: Create invoices for customers and see how they're linked
4. **Try Search**: Use the search endpoint with different parameters
5. **Test Pagination**: Request different pages and page sizes
6. **Review Logs**: Watch console output to see logging in action

## Additional Resources

- **Swagger Documentation**: http://localhost:5000 (when running)
- **Dapper Documentation**: https://github.com/DapperLib/Dapper
- **ASP.NET Core Documentation**: https://docs.microsoft.com/aspnet/core/

Happy API testing!
