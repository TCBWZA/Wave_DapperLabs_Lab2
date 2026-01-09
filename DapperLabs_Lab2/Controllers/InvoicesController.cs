using DapperLabs_Lab2.DTOs;
using DapperLabs_Lab2.Mappings;
using DapperLabs_Lab2.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DapperLabs_Lab2.Controllers
{
    /// <summary>
    /// API endpoints for managing invoices
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<InvoicesController> _logger;

        public InvoicesController(
            IInvoiceRepository invoiceRepository,
            ICustomerRepository customerRepository,
            ILogger<InvoicesController> logger)
        {
            _invoiceRepository = invoiceRepository;
            _customerRepository = customerRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all invoices
        /// </summary>
        /// <returns>List of all invoices</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetAll()
        {
            _logger.LogInformation("Getting all invoices");

            var invoices = await _invoiceRepository.GetAllAsync();
            return Ok(invoices.Select(i => i.ToDto()));
        }

        /// <summary>
        /// Get a specific invoice by ID
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>Invoice details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceDto>> GetById(long id)
        {
            _logger.LogInformation("Getting invoice {InvoiceId}", id);

            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found", id);
                return NotFound(new { message = $"Invoice with ID {id} not found" });
            }

            return Ok(invoice.ToDto());
        }

        /// <summary>
        /// Get all invoices for a specific customer
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <returns>List of customer invoices</returns>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(typeof(IEnumerable<InvoiceDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<InvoiceDto>>> GetByCustomerId(long customerId)
        {
            _logger.LogInformation("Getting invoices for customer {CustomerId}", customerId);

            if (!await _customerRepository.ExistsAsync(customerId))
            {
                _logger.LogWarning("Customer {CustomerId} not found", customerId);
                return NotFound(new { message = $"Customer with ID {customerId} not found" });
            }

            var invoices = await _invoiceRepository.GetByCustomerIdAsync(customerId);
            return Ok(invoices.Select(i => i.ToDto()));
        }

        /// <summary>
        /// Create a new invoice
        /// </summary>
        /// <param name="dto">Invoice creation data</param>
        /// <returns>Created invoice</returns>
        [HttpPost]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvoiceDto>> Create([FromBody] CreateInvoiceDto dto)
        {
            _logger.LogInformation("Creating invoice - Number: {InvoiceNumber}, CustomerId: {CustomerId}", 
                dto.InvoiceNumber, dto.CustomerId);

            // Check if customer exists
            if (!await _customerRepository.ExistsAsync(dto.CustomerId))
            {
                _logger.LogWarning("Customer {CustomerId} not found", dto.CustomerId);
                return BadRequest(new { message = $"Customer with ID {dto.CustomerId} does not exist" });
            }

            // Check if invoice number already exists
            if (await _invoiceRepository.InvoiceNumberExistsAsync(dto.InvoiceNumber))
            {
                _logger.LogWarning("Invoice number {InvoiceNumber} already exists", dto.InvoiceNumber);
                return BadRequest(new { message = $"Invoice number '{dto.InvoiceNumber}' already exists" });
            }

            var invoice = dto.ToEntity();
            var created = await _invoiceRepository.CreateAsync(invoice);

            _logger.LogInformation("Invoice created with ID: {InvoiceId}", created.Id);

            return CreatedAtAction(
                nameof(GetById), 
                new { id = created.Id }, 
                created.ToDto());
        }

        /// <summary>
        /// Update an existing invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <param name="dto">Invoice update data</param>
        /// <returns>Updated invoice</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(InvoiceDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvoiceDto>> Update(long id, [FromBody] UpdateInvoiceDto dto)
        {
            _logger.LogInformation("Updating invoice {InvoiceId}", id);

            var invoice = await _invoiceRepository.GetByIdAsync(id);
            if (invoice == null)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for update", id);
                return NotFound(new { message = $"Invoice with ID {id} not found" });
            }

            // Check if invoice number already exists for another invoice
            if (await _invoiceRepository.InvoiceNumberExistsAsync(dto.InvoiceNumber, id))
            {
                _logger.LogWarning("Invoice number {InvoiceNumber} already exists for another invoice", dto.InvoiceNumber);
                return BadRequest(new { message = $"Invoice number '{dto.InvoiceNumber}' is already in use by another invoice" });
            }

            dto.UpdateEntity(invoice);
            var updated = await _invoiceRepository.UpdateAsync(invoice);

            _logger.LogInformation("Invoice {InvoiceId} updated successfully", id);

            return Ok(updated.ToDto());
        }

        /// <summary>
        /// Delete an invoice
        /// </summary>
        /// <param name="id">Invoice ID</param>
        /// <returns>No content on success</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            _logger.LogInformation("Deleting invoice {InvoiceId}", id);

            var deleted = await _invoiceRepository.DeleteAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Invoice {InvoiceId} not found for deletion", id);
                return NotFound(new { message = $"Invoice with ID {id} not found" });
            }

            _logger.LogInformation("Invoice {InvoiceId} deleted successfully", id);

            return NoContent();
        }
    }
}
