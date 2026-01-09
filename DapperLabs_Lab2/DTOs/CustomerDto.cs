using System.ComponentModel.DataAnnotations;

namespace DapperLabs_Lab2.DTOs
{
    // [API] DTO (Data Transfer Object) - API-specific model for responses
    public class CustomerDto
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public decimal Balance { get; set; } // [API] Calculated property for API response
        public List<InvoiceDto>? Invoices { get; set; } // [API] Related data included when requested
        public List<TelephoneNumberDto>? PhoneNumbers { get; set; } // [API] Related data included when requested
    }

    // [API] CreateDto - API-specific model for POST requests (no ID)
    public class CreateCustomerDto
    {
        // [API] Data Annotations - API validation attributes
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")] // [API] Email validation
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;
    }

    // [API] UpdateDto - API-specific model for PUT requests (ID in URL)
    public class UpdateCustomerDto
    {
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [StringLength(200, ErrorMessage = "Email cannot exceed 200 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}
