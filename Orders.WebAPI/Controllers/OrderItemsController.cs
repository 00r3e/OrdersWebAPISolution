using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.WebAPI.ApplicationDbContext;
using Entities;
using Microsoft.IdentityModel.Tokens;
using ServiceContracts.IOrderItemsServices;
using ServiceContracts.IOrdersServices;
using ServiceContracts.DTO.OrderDTO;
using ServiceContracts.DTO.OrderItemDTO;

namespace Orders.WebAPI.Controllers
{
    public class OrderItemsController : CustomControllerBase
    {
        private readonly IOrderItemsAdderService _orderItemsAdderService;
        private readonly IOrderItemsDeleterService _orderItemsDeleterService;
        private readonly IOrderItemsGetterService _orderItemsGetterService;
        private readonly IOrderItemsUpdaterService _orderItemsUpdaterService;
        private readonly IOrdersGetterService _orderGetterService;
        private readonly IOrderItemsBatchService _orderItemsBatchService;
        private readonly ILogger<OrderItemsController> _logger;

        public OrderItemsController(ILogger<OrderItemsController> logger, IOrderItemsDeleterService orderItemsDeleterService, IOrderItemsAdderService orderItemsAdderService, 
            IOrderItemsGetterService orderItemsGetterService, IOrderItemsUpdaterService orderItemsUpdaterService, IOrdersGetterService orderGetterService, IOrderItemsBatchService orderItemsBatchService)
        {
            _logger = logger;
            _orderItemsDeleterService = orderItemsDeleterService;
            _orderItemsGetterService = orderItemsGetterService;
            _orderItemsUpdaterService = orderItemsUpdaterService;
            _orderItemsAdderService = orderItemsAdderService;
            _orderGetterService = orderGetterService;
            _orderItemsBatchService = orderItemsBatchService;
        }

        // GET: api/orders/{orderId}/items
        [HttpGet("orders/{orderId}/items")]
        public async Task<ActionResult<ICollection<OrderItemResponse>>> GetOrderItemsFromOrderId(Guid orderId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetOrderItemsFromOrderId), nameof(OrderItemsController));

            if (orderId == Guid.Empty)
            {
                return BadRequest();
            }
            OrderResponse? orderResponse = await _orderGetterService.GetOrder(orderId);
            if (orderResponse == null)
            {
                return BadRequest("OrderId does not match with any Order Id from Order Database.");
            }

            var orderItemResponses = await _orderItemsGetterService.GetOrderItemsFromOrderID(orderId);

            return Ok(orderItemResponses);
        }

        // GET: api/orders/{orderId}/items/{orderItemId}
        [HttpGet("orders/{orderId}/items/{orderItemId}")]
        public async Task<ActionResult<OrderItemResponse>> GetOrderItem(Guid orderId, Guid orderItemId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(GetOrderItem), nameof(OrderItemsController));


            if (orderId == Guid.Empty || orderItemId == Guid.Empty)
            {
                return BadRequest();
            }
            OrderResponse? orderResponse = await _orderGetterService.GetOrder(orderId);
            if (orderResponse == null)
            {
                return BadRequest("OrderId does not match with any Order Id from Order Database.");
            }

            var orderItemResponse = await _orderItemsGetterService.GetOrderItem(orderId, orderItemId);

            if (orderItemResponse == null)
            {
                return NotFound();
            }

            return Ok(orderItemResponse);
        }

        // PUT:  /api/orders/{orderId}/items/{orderItemId}
        [HttpPut("orders/{orderId}/items/{orderItemId}")]
        public async Task<IActionResult> PutOrderItem(Guid orderId, Guid orderItemId,[FromBody] OrderItemUpdateRequest orderItemUpdateRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PutOrderItem), nameof(OrderItemsController));

            if (orderItemUpdateRequest == null)
            {
                return BadRequest("Order data is missing.");
            }

            if (orderItemId != orderItemUpdateRequest.OrderItemId)
            {
                return BadRequest("orderItemId does not match");
            }

            OrderResponse? orderResponse = await _orderGetterService.GetOrder(orderId);
            if (orderResponse == null)
            {
                return NotFound("Order Id not found");
            }

            OrderItemResponse? orderItemResponse = await _orderItemsGetterService.GetOrderItem(orderId ,orderItemId);
            if (orderItemResponse == null)
            {
                return NotFound("Order Item Id not found");
            }

            var OrderItemResponseFromAdd = await _orderItemsUpdaterService.UpdateOrderItem(orderId, orderItemUpdateRequest);

            if (OrderItemResponseFromAdd == null)
            {
                return Problem("An error occurred while saving the order.");
            }

            return CreatedAtAction(nameof(GetOrderItem), new { orderId = OrderItemResponseFromAdd.OrderId, orderItemId = OrderItemResponseFromAdd.OrderId }, OrderItemResponseFromAdd);
        }

        // POST: /api/orders/{orderId}/items
        [HttpPost("orders/{orderId}/items")]
        public async Task<ActionResult> PostOrderItem([FromRoute]Guid orderId, [FromBody] OrderItemAddRequest orderItemAddRequest)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(PostOrderItem), nameof(OrderItemsController));

            if (orderItemAddRequest == null)
            {
                return BadRequest("Order data is missing.");
            }

            if (orderId != orderItemAddRequest.OrderId)
            {
                return BadRequest("Order Id from Route does not match with any Order Id from Body.");
            }

            OrderResponse? orderResponse = await _orderGetterService.GetOrder(orderItemAddRequest.OrderId);
            if (orderResponse == null)
            {
                return BadRequest("OrderId does not match with any Order Id from Order Database.");
            }

            var OrderItemResponseFromAdd = await _orderItemsAdderService.AddOrderItem(orderItemAddRequest);

            if (OrderItemResponseFromAdd == null)
            {
                return Problem("An error occurred while saving the order.");
            }

            return CreatedAtAction(nameof(GetOrderItem), new { orderId = OrderItemResponseFromAdd.OrderId, orderItemId = OrderItemResponseFromAdd.OrderItemId }, OrderItemResponseFromAdd);
        }

        // DELETE: /api/orders/{orderId}/items/{orderItemId}
        [HttpDelete("orders/{orderId}/items/{orderItemId}")]
        public async Task<IActionResult> DeleteOrderItem([FromRoute]Guid orderId,[FromRoute] Guid orderItemId)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(DeleteOrderItem), nameof(OrderItemsController));


            var orderItemResponse = await _orderItemsGetterService.GetOrderItem(orderId, orderItemId);
            if (orderItemResponse == null)
            {
                return NotFound();
            }

            var isDeleted = await _orderItemsDeleterService.DeleteOrderItem(orderId, orderItemId);
            if (!isDeleted)
            {
                return Problem("An error occurred while deleting the order.");
            }

            return NoContent();
        }

        // POST: /api/orders/{orderId}/items
        [HttpPost("orders/{orderId}/items/batch")]
        public async Task<ActionResult> BatchOrderItems([FromRoute] Guid orderId, [FromBody] List<OrderItemAddRequest> orderItemAddRequests)
        {
            _logger.LogInformation("{MetodName} action method of {ControllerName}", nameof(BatchOrderItems), nameof(OrderItemsController));

            if(orderItemAddRequests.Any(oi => oi.OrderId != orderId))
            {
                return BadRequest("Order Id from Route does not match with all Order Ids from Body.");
            }

            if (!orderItemAddRequests.Any())
            {
                return BadRequest("Order Items data is missing.");
            }


            OrderResponse? orderResponse = await _orderGetterService.GetOrder(orderId);
            if (orderResponse == null)
            {
                return BadRequest("Order Id does not match with any Order Id from Order Database.");
            }

            var OrderItemResponseFromAdd = await _orderItemsBatchService.CreateOrderItems(orderItemAddRequests);

            if (OrderItemResponseFromAdd == null)
            {
                return Problem("An error occurred while saving the order.");
            }

            return CreatedAtAction(nameof(GetOrderItemsFromOrderId), new { orderId = orderId}, OrderItemResponseFromAdd);
        }

    }
}
