using DapperLabs_Lab2.DTOs;
using DapperLabs_Lab2.Mappings;
using DapperLabs_Lab2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DapperLabs_Lab2.Controllers
{
    /// <summary>
    /// API endpoints for managing telephone numbers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class TelephoneNumbersController : ControllerBase
    {
        private readonly ITelephoneNumberRepository _telephoneRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<TelephoneNumbersController> _logger;

        public TelephoneNumbersController(
            ITelephoneNumberRepository telephoneRepository,
            ICustomerRepository customerRepository,
            ILogger<TelephoneNumbersController> logger)
        {
            _telephoneRepository = telephoneRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all telephone numbers
        /// </summary>
        /// <returns>List of all telephone numbers</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TelephoneNumberDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TelephoneNumberDto>>> GetAll()
        {
            _logger.LogInformation("Getting all telephone numbers");

            var phoneNumbers = await _telephoneRepository.GetAllAsync();
            return Ok(phoneNumbers.Select(p => p.ToDto()));
        }

        /// <summary>
        /// Get a specific telephone number by ID
        /// </summary>
        /// <param name="id">Telephone number ID</param>
        /// <returns>Telephone number details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TelephoneNumberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TelephoneNumberDto>> GetById(long id)
        {
            _logger.LogInformation("Getting telephone number {PhoneId}", id);

            var phoneNumber = await _telephoneRepository.GetByIdAsync(id);
            if (phoneNumber == null)
            {
                _logger.LogWarning("Telephone number {PhoneId} not found", id);
                return NotFound(new { message = $"Telephone number with ID {id} not found" });
            }

            return Ok(phoneNumber.ToDto());
        }

        /// <summary>
        /// Get all telephone numbers for a specific customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer telephone numbers</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<TelephoneNumberDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TelephoneNumberDto>>> GetByCustomerId(long customerId)
        {
            _logger.LogInformation("Getting telephone numbers for customer {CustomerId}", customerId);

            if (!await _customerRepository.ExistsAsync(customerId))
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                return NotFound(new { message = $"Customer with ID {customerId} not found" });
            }

            var phoneNumbers = await _telephoneRepository.GetByCustomerIdAsync(customerId);
            return Ok(phoneNumbers.Select(p => p.ToDto()));
        }

        /// <summary>
        /// Create a new telephone number
        /// </summary>
        /// <param name="dto">Telephone number creation data</param>
        /// <returns>Created telephone number</returns>
        [HttpPost]
        [ProducesResponseType(typeof(TelephoneNumberDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TelephoneNumberDto>> Create([FromBody] CreateTelephoneNumberDto dto)
        {
            _logger.LogInformation("Creating telephone number for customer {CustomerId}", dto.CustomerId);

            // Check if customer exists
            if (!await _customerRepository.ExistsAsync(dto.CustomerId))
            {
                _logger.LogWarning("Customer {CustomerId} not found", dto.CustomerId);
                return BadRequest(new { message = $"Customer with ID {dto.CustomerId} does not exist" });
            }

            var phoneNumber = dto.ToEntity();
            var created = await _telephoneRepository.CreateAsync(phoneNumber);

            _logger.LogInformation("Telephone number created with ID: {PhoneId}", created.Id);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created.ToDto());
        }

        /// <summary>
        /// Update an existing telephone number
        /// </summary>
        /// <param name="id">Telephone number ID</param>
        /// <param name="dto">Telephone number update data</param>
        /// <returns>Updated telephone number</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(TelephoneNumberDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TelephoneNumberDto>> Update(long id, [FromBody] UpdateTelephoneNumberDto dto)
        {
            _logger.LogInformation("Updating telephone number {PhoneId}", id);

            var phoneNumber = await _telephoneRepository.GetByIdAsync(id);
            if (phoneNumber == null)
            {
                _logger.LogWarning("Telephone number {PhoneId} not found for update", id);
                return NotFound(new { message = $"Telephone number with ID {id} not found" });
            }

            dto.UpdateEntity(phoneNumber);
            var updated = await _telephoneRepository.UpdateAsync(phoneNumber);

            _logger.LogInformation("Telephone number {PhoneId} updated successfully", id);

            return Ok(updated.ToDto());
        }

        /// <summary>
        /// Delete a telephone number
        /// </summary>
        /// <param name="id">Telephone number ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting telephone number {PhoneId}", id);

            var deleted = await _telephoneRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Telephone number {PhoneId} not found for deletion", id);
                return NotFound(new { message = $"Telephone number with ID {id} not found" });
            }

            _logger.LogInformation("Telephone number {PhoneId} deleted successfully", id);

            return NoContent();
        }
    }
}
