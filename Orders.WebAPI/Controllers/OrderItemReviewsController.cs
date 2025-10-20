using Microsoft.AspNetCore.Mvc;
using ServiceContracts.DTO.OrderItemReviewDTO;
using ServiceContracts.IOrderItemReviewsServices;
using ServiceContracts.IOrderItemsServices;
using ServiceContracts.IOrdersServices;
using Services.OrderItemsReviewsServices;


namespace Orders.WebAPI.Controllers
{
    public class OrderItemReviewsController : CustomControllerBase
    {
        private readonly IOrderItemReviewsAdderService _orderItemReviewsAdderService;
        private readonly IOrderItemReviewsDeleterService _orderItemReviewsDeleterService;
        private readonly IOrderItemReviewsGetterService _orderItemReviewsGetterService;
        private readonly IOrderItemReviewsUpdaterService _orderItemReviewsUpdaterService;
        private readonly IOrderItemsGetterService _orderItemsGetterService;
        private readonly IOrdersGetterService _orderGetterService;

        private readonly ILogger<OrderItemReviewsController> _logger;

        public OrderItemReviewsController(ILogger<OrderItemReviewsController> logger, IOrderItemReviewsDeleterService orderItemReviewsDeleterService, IOrderItemReviewsAdderService orderItemReviewsAdderService,
            IOrderItemReviewsGetterService orderItemReviewsGetterService, IOrderItemReviewsUpdaterService orderItemReviewsUpdaterService, IOrderItemsGetterService orderItemsGetterService, IOrdersGetterService orderGetterService)
        {
            _logger = logger;
            _orderItemReviewsDeleterService = orderItemReviewsDeleterService;
            _orderItemReviewsGetterService = orderItemReviewsGetterService;
            _orderItemReviewsUpdaterService = orderItemReviewsUpdaterService;
            _orderItemReviewsAdderService = orderItemReviewsAdderService;
            _orderItemsGetterService = orderItemsGetterService;
            _orderGetterService = orderGetterService;

        }

        // GET: /api/orders/{orderId}/items/{orderItemId}/review
        [HttpGet("orders/{orderId}/items/{orderItemId}/review")]
        public async Task<ActionResult<OrderItemReviewResponse>> GetOrderItemReview(Guid orderId, Guid orderItemId)
        {
            _logger.LogInformation("GetReview called for OrderItemId: {orderItemId}", orderItemId);

            if (orderId == Guid.Empty || orderItemId == Guid.Empty)
                return BadRequest("Invalid orderId or orderItemId");

            var orderItem = await _orderGetterService.GetOrder(orderId);
            if (orderItem == null)
                return NotFound("Order not found");

            OrderItemReviewResponse? orderItemReview = await _orderItemReviewsGetterService.GetOrderItemReview(orderId, orderItemId);
            if (orderItemReview == null)
                return NotFound("Review not found");

            return Ok(orderItemReview);
        }

        // POST: /api/orders/{orderId}/items/{orderItemId}/review
        [HttpPost("orders/{orderId}/items/{orderItemId}/review")]
        public async Task<ActionResult<OrderItemReviewResponse>> CreateReview(Guid orderId, Guid orderItemId, [FromBody] OrderItemReviewAddRequest orderItemReviewAddRequest)
        {
            _logger.LogInformation("CreateReview called for OrderItemId: {orderItemId}", orderItemId);

            if (orderItemReviewAddRequest == null)
                return BadRequest("Review data is missing");

            if (orderItemReviewAddRequest.OrderItemId != orderItemId)
                return BadRequest("OrderItemId in body does not match route");

            var existingOrderResponse = await _orderGetterService.GetOrder(orderId);
            if (existingOrderResponse == null)
                return NotFound("Order not found");

            var existingOrderItemResponse = await _orderItemsGetterService.GetOrderItem(orderId, orderItemId);
            if (existingOrderItemResponse == null)
                return NotFound("Review item not found");

            var existingReview = await _orderItemReviewsGetterService.GetOrderItemReview(orderId, orderItemId);
            if (existingReview != null)
                return Conflict("Review already exists for this order item");

            var createdReview = await _orderItemReviewsAdderService.AddOrderItemReview(existingOrderResponse.CustomerId, orderItemReviewAddRequest);
            if (createdReview == null)
                return Problem("Error creating review");

            return CreatedAtAction(nameof(GetOrderItemReview), new { orderId = orderId, orderItemId = orderItemId }, createdReview);
        }

        // PUT: /api/orders/{orderId}/items/{orderItemId}/review
        [HttpPut("orders/{orderId}/items/{orderItemId}/review")]
        public async Task<ActionResult<OrderItemReviewResponse>> UpdateOrderItemReview(Guid orderId, Guid orderItemId, [FromBody] OrderItemReviewUpdateRequest orderItemReviewUpdateRequest)
        {
            _logger.LogInformation("UpdateReview called for OrderItemId: {orderItemId}", orderItemId);

            if (orderItemReviewUpdateRequest == null)
                return BadRequest("Update data is missing");

            if (orderItemReviewUpdateRequest.OrderItemId != orderItemId)
                return BadRequest("OrderItemId in body does not match route");

            var order = await _orderGetterService.GetOrder(orderId);
            if (order == null)
                return NotFound("Order not found");

            var orderItem = await _orderItemsGetterService.GetOrderItem(orderId, orderItemId);
            if (orderItem == null)
                return NotFound("Order item not found");

            var orderItemReview = await _orderItemReviewsGetterService.GetOrderItemReview(orderId, orderItemId);
            if (orderItemReview == null)
                return NotFound("Order item review not found");

            var updatedReview = await _orderItemReviewsUpdaterService.UpdateOrderItemReview(order.CustomerId, orderItemReviewUpdateRequest);
            if (updatedReview == null)
                return Problem("Error updating review");

            return Ok(updatedReview);
        }

        // DELETE: /api/orders/{orderId}/items/{orderItemId}/review
        [HttpDelete("orders/{orderId}/items/{orderItemId}/review")]
        public async Task<IActionResult> DeleteReview(Guid orderId, Guid orderItemId)
        {
            _logger.LogInformation("DeleteReview called for OrderItemId: {orderItemId}", orderItemId);

            var order = await _orderGetterService.GetOrder(orderId);
            if (order == null)
                return NotFound("Ordernot found");

            var orderItem = await _orderItemsGetterService.GetOrderItem(orderId, orderItemId);
            if (orderItem == null)
                return NotFound("Order item not found");

            var orderItemReview = await _orderItemReviewsGetterService.GetOrderItemReview(orderId, orderItemId);
            if (orderItemReview == null)
                return NotFound("Order item review not found");

            var deleted = await _orderItemReviewsDeleterService.DeleteOrderItemReview(orderId, orderItemId);
            if (!deleted)
                return Problem("Error deleting order item review");

            return NoContent();
        }
    }
}
