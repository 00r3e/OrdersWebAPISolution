using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Humanizer.Localisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using ServiceContracts.DTO.CustomersDTO;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.ICustomersServices;
using ServiceContracts.IOrdersServices;

namespace Orders.WebAPI.Controllers
{
    public class OrdersController : CustomControllerBase
    {
        private readonly IOrdersAdderService _ordersAdderService;
        private readonly IOrdersDeleterService _ordersDeleterService;
        private readonly IOrdersGetterService _ordersGetterService;
        private readonly IOrdersUpdaterService _ordersUpdaterService;
        private readonly IOrdersBatchService _ordersBatchService;
        private readonly ICustomersGetterService _customersGetterService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrdersAdderService ordersAdderService, IOrdersDeleterService ordersDeleterService, IOrdersGetterService ordersGetterService, 
            IOrdersUpdaterService ordersUpdaterService,IOrdersBatchService ordersBatchService, ICustomersGetterService customersGetterService, ILogger<OrdersController> logger)
        {
            _ordersAdderService = ordersAdderService;
            _logger = logger;
            _ordersDeleterService = ordersDeleterService;
            _ordersGetterService = ordersGetterService;
            _ordersUpdaterService = ordersUpdaterService;
            _ordersBatchService = ordersBatchService;
            _customersGetterService = customersGetterService;
        }

        // GET: api/Orders
        [HttpGet("orders")]
        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrders()
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetOrders), nameof(OrdersController));
            var orderRespones = await _ordersGetterService.GetOrders();
            return Ok(orderRespones);
        }

        // GET: api/Orders/5
        [HttpGet("orders/{orderId}")]
        public async Task<ActionResult<OrderResponse>> GetOrder(Guid orderId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetOrder), nameof(OrdersController));

            var orderResponse = await _ordersGetterService.GetOrder(orderId);

            if (orderResponse == null)
            {
                return NotFound();
            }

            return orderResponse;
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("orders/{orderId}")]
        public async Task<IActionResult> PutOrder(Guid orderId, OrderUpdateRequest orderUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PutOrder), nameof(OrdersController));

            if (orderId != orderUpdateRequest.OrderId)
            {
                return BadRequest("Order ID mismatch.");
            }

            var existingOrder = await _ordersGetterService.GetOrder(orderId);
            if (existingOrder == null)
            {
                return NotFound("Order Id not found");
            }

            CustomerResponse? customerResponse = await _customersGetterService.GetCustomer(orderUpdateRequest.CustomerId);
            if (customerResponse == null) 
            {
                return NotFound("Customer Id not found");
            }

            try
            {
                OrderResponse? updatedOrderResponse = await _ordersUpdaterService.UpdateOrder(orderUpdateRequest);
                if (updatedOrderResponse is null)
                {
                    return Problem("Failed to update the order.");
                }

                return CreatedAtAction(nameof(GetOrder), new { orderId = updatedOrderResponse.OrderId}, updatedOrderResponse);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("A concurrency conflict occurred.");
            }

            
        }

        // POST: api/Orders
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost("orders")]
        public async Task<ActionResult> PostOrder(OrderAddRequest orderAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PostOrder), nameof(OrdersController));

            if (orderAddRequest == null)
            {
                return BadRequest("Order data is missing.");
            }

            var orderResponse = await _ordersAdderService.AddOrder(orderAddRequest);

            if (orderResponse == null)
            {
                return Problem("An error occurred while saving the order.");
            }

            return CreatedAtAction(nameof(GetOrder), new { orderId = orderResponse.OrderId }, orderResponse);
        }

        // DELETE: api/Orders/5
        [HttpDelete("orders/{orderId}")]
        public async Task<IActionResult> DeleteOrder(Guid orderId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(DeleteOrder), nameof(OrdersController));

            var order = await _ordersGetterService.GetOrder(orderId);
            if (order == null)
            {
                return NotFound();
            }

            bool isDeleted = await _ordersDeleterService.DeleteOrder(orderId);
            if (!isDeleted)
            {
                return Problem("Failed to delete the order.");
            }

            return NoContent();
        }

        // Batch: api/Orders/batch
        [HttpPost("orders/batch")]
        public async Task<IActionResult> BatchOrders([FromBody] List<OrderAddRequest> orderAddRequests)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(BatchOrders), nameof(OrdersController));

            if (!orderAddRequests.Any())
            {
                return BadRequest("The list of order add requests cannot be empty");
            }

            var orderResponses = await _ordersBatchService.CreateOrders(orderAddRequests);

            return CreatedAtAction(nameof(GetOrders), orderResponses);
        }

    }
}
