using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.ICustomersServices;
using ServiceContracts.IOrdersServices;

namespace Orders.WebAPI.Controllers
{
    public class CustomersController : CustomControllerBase
    {
        private readonly ICustomersAdderService _customersAdderService;
        private readonly ICustomersDeleterService _customersDeleterService;
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ICustomersUpdaterService _customersUpdaterService;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(ICustomersAdderService customersAdderService, ICustomersDeleterService customersDeleterService, ICustomersGetterService customersGetterService,
            ICustomersUpdaterService customersUpdaterService, ILogger<CustomersController> logger)
        {
            _customersAdderService = customersAdderService;
            _customersDeleterService = customersDeleterService;
            _customersGetterService = customersGetterService;
            _customersUpdaterService = customersUpdaterService;
            _logger = logger;
        }

        // GET: api/Customers
        [HttpGet("customers")]
        public async Task<ActionResult<IEnumerable<CustomerResponse>>> GetCustomers()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetCustomers), nameof(CustomersController));
            var customersRespones = await _customersGetterService.GetAllCustomers();
            return Ok(customersRespones);
        }

        // GET: api/customers/5
        [HttpGet("customers/{customerId}")]
        public async Task<ActionResult<CustomerResponse>> GetCustomer(Guid customerId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetCustomer), nameof(CustomersController));

            var customerResponse = await _customersGetterService.GetCustomer(customerId);

            if (customerResponse == null)
            {
                return NotFound();
            }

            return customerResponse;
        }

        // PUT: api/customers/5
        [HttpPut("customers/{customerId}")]
        public async Task<IActionResult> PutCustomer(Guid customerId, CustomerUpdateRequest customerUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PutCustomer), nameof(CustomersController));

            if (customerId != customerUpdateRequest.CustomerId)
            {
                return BadRequest("Customer ID mismatch.");
            }

            var existingCustomer = await _customersGetterService.GetCustomer(customerId);
            if (existingCustomer == null)
            {
                return NotFound();
            }

            try
            {
                CustomerResponse? customerOrderResponse = await _customersUpdaterService.UpdateCustomer(customerUpdateRequest);
                if (customerOrderResponse is null)
                {
                    return Problem("Failed to update the customer.");
                }

                return CreatedAtAction(nameof(GetCustomer), new {customerId = customerOrderResponse.CustomerId }, customerOrderResponse);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency conflict occurred.");
            }
        }

        // POST: api/Customers
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("customers")]
        public async Task<ActionResult> PostCustomer(CustomerAddRequest customerAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PostCustomer), nameof(CustomersController));

            if (customerAddRequest == null)
            {
                return BadRequest("Customer data is missing.");
            }

            var customerResponse = await _customersAdderService.AddCustomer(customerAddRequest);

            if (customerResponse == null)
            {
                return Problem("An error occurred while saving the customer.");
            }

            return CreatedAtAction(nameof(GetCustomer), new { customerId = customerResponse.CustomerId }, customerResponse);
        }

        // DELETE: api/Customers/5
        [HttpDelete("customers/{customerId}")]
        public async Task<IActionResult> DeleteCustomer(Guid customerId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(DeleteCustomer), nameof(CustomersController));

            var customer = await _customersGetterService.GetCustomer(customerId);
            if (customer == null)
            {
                return NotFound();
            }

            bool isDeleted = await _customersDeleterService.DeleteCustomer(customerId);
            if (!isDeleted)
            {
                return Problem("Failed to delete the customer.");
            }

            return NoContent();
        }
    }
}
