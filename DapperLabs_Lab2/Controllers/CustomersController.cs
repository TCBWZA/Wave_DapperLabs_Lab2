using DapperLabs_Lab2.DTOs;
using DapperLabs_Lab2.Mappings;
using DapperLabs_Lab2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DapperLabs_Lab2.Controllers
{
    /// <summary>
    /// API endpoints for managing customers
    /// </summary>
    // [API] Controller attributes - API-specific attributes for routing and response formatting
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CustomersController : ControllerBase // [API] ControllerBase - Base class for API controllers
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersController> _logger; // [API] ILogger - API-specific logging interface

        public CustomersController(
            ICustomerRepository customerRepository,
            ILogger<CustomersController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all customers with optional pagination and related data
        /// </summary>
        /// <param name="page">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10, max: 100)</param>
        /// <param name="includeRelated">Include invoices and phone numbers (default: false)</param>
        /// <returns>Paginated list of customers</returns>
        // [API] HTTP verb attribute and response type documentation
        [HttpGet]
        [ProducesResponseType(typeof(PagedResultDto<CustomerDto>), StatusCodes.Status200OK)]
        // [API] ActionResult<T> - API-specific return type wrapper
        public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetAll(
            [FromQuery] int page = 1, // [API] FromQuery - Binds parameters from query string
            [FromQuery] int pageSize = 10,
            [FromQuery] bool includeRelated = false)
        {
            // [API] Logging - API-specific logging for request tracking
            _logger.LogInformation("Getting customers - Page: {Page}, PageSize: {PageSize}, IncludeRelated: {IncludeRelated}", 
                page, pageSize, includeRelated);

            // [API] Input validation for API parameters
            if (pageSize > 100) pageSize = 100;
            if (page < 1) page = 1;

            // [DAPPER] Repository call - Dapper executes SQL in repository
            var (customers, totalCount) = await _customerRepository.GetPagedAsync(page, pageSize, includeRelated);

            // [API] PagedResultDto - API-specific pagination response model
            var result = new PagedResultDto<CustomerDto>
            {
                Items = customers.Select(c => c.ToDto()),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            // [API] Ok() - Returns HTTP 200 with JSON payload
            return Ok(result);
        }

        /// <summary>
        /// Get a specific customer by ID
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="includeRelated">Include invoices and phone numbers (default: false)</param>
        /// <returns>Customer details</returns>
        // [API] Route parameter binding with {id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerDto>> GetById(long id, [FromQuery] bool includeRelated = false)
        {
            _logger.LogInformation("Getting customer {CustomerId}, IncludeRelated: {IncludeRelated}", id, includeRelated);

            // [DAPPER] Repository call - Dapper executes parameterized SQL query
            var customer = await _customerRepository.GetByIdAsync(id, includeRelated);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found", id);
                // [API] NotFound() - Returns HTTP 404 with error message
                return NotFound(new { message = $"Customer with ID {id} not found" });
            }

            // [API] Ok() with DTO mapping for API response
            return Ok(customer.ToDto());
        }

        /// <summary>
        /// Search customers by name, email, or minimum balance
        /// </summary>
        /// <param name="name">Search by name (partial match)</param>
        /// <param name="email">Search by email (partial match)</param>
        /// <param name="minBalance">Minimum balance filter</param>
        /// <returns>List of matching customers</returns>
        // [API] Custom route for search functionality
        [HttpGet("search")]
        [ProducesResponseType(typeof(IEnumerable<CustomerDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> Search(
            [FromQuery] string? name = null,
            [FromQuery] string? email = null,
            [FromQuery] decimal? minBalance = null)
        {
            _logger.LogInformation("Searching customers - Name: {Name}, Email: {Email}, MinBalance: {MinBalance}", 
                name, email, minBalance);

            // [DAPPER] Dynamic SQL with parameters - Dapper builds WHERE clause dynamically
            var customers = await _customerRepository.SearchAsync(name, email, minBalance);
            return Ok(customers.Select(c => c.ToDto()));
        }

        /// <summary>
        /// Create a new customer
        /// </summary>
        /// <param name="dto">Customer creation data</param>
        /// <returns>Created customer</returns>
        // [API] HttpPost for creation
        [HttpPost]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> Create([FromBody] CreateCustomerDto dto) // [API] FromBody - Deserializes JSON to DTO
        {
            _logger.LogInformation("Creating customer - Email: {Email}", dto.Email);

            // [API] Business rule validation before creation
            // [DAPPER] Repository call - Dapper checks if email exists
            if (await _customerRepository.EmailExistsAsync(dto.Email))
            {
                _logger.LogWarning("Email {Email} already exists", dto.Email);
                // [API] BadRequest() - Returns HTTP 400 with validation error
                return BadRequest(new { message = $"Email '{dto.Email}' is already in use" });
            }

            var customer = dto.ToEntity();
            // [DAPPER] Repository call - Dapper INSERT with SCOPE_IDENTITY to get auto-generated ID
            var created = await _customerRepository.CreateAsync(customer);

            _logger.LogInformation("Customer created with ID: {CustomerId}", created.Id);

            // [API] CreatedAtAction() - Returns HTTP 201 with Location header and created resource
            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created.ToDto());
        }

        /// <summary>
        /// Update an existing customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <param name="dto">Customer update data</param>
        /// <returns>Updated customer</returns>
        // [API] HttpPut for full resource update
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CustomerDto>> Update(long id, [FromBody] UpdateCustomerDto dto)
        {
            _logger.LogInformation("Updating customer {CustomerId}", id);

            // [DAPPER] Repository call - Dapper SELECT to verify existence
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                _logger.LogWarning("Customer {CustomerId} not found for update", id);
                return NotFound(new { message = $"Customer with ID {id} not found" });
            }

            // [API] Business rule validation - check email uniqueness
            // [DAPPER] Repository call - Dapper checks email with exclusion
            if (await _customerRepository.EmailExistsAsync(dto.Email, id))
            {
                _logger.LogWarning("Email {Email} already exists for another customer", dto.Email);
                return BadRequest(new { message = $"Email '{dto.Email}' is already in use by another customer" });
            }

            dto.UpdateEntity(customer);
            // [DAPPER] Repository call - Dapper UPDATE statement
            var updated = await _customerRepository.UpdateAsync(customer);

            _logger.LogInformation("Customer {CustomerId} updated successfully", id);

            return Ok(updated.ToDto());
        }

        /// <summary>
        /// Delete a customer
        /// </summary>
        /// <param name="id">Customer ID</param>
        /// <returns>No content on success</returns>
        // [API] HttpDelete for resource deletion
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting customer {CustomerId}", id);

            // [DAPPER] Repository call - Dapper DELETE statement
            var deleted = await _customerRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Customer {CustomerId} not found for deletion", id);
                return NotFound(new { message = $"Customer with ID {id} not found" });
            }

            _logger.LogInformation("Customer {CustomerId} deleted successfully", id);

            // [API] NoContent() - Returns HTTP 204 with no body
            return NoContent();
        }
    }
}
