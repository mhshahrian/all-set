using AllSet.Domain;
using AllSet.Services;
using Microsoft.AspNetCore.Mvc;
using static AllSet.DTOs.DataTransferObjects;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AllSet.Controllers
{
    [Route("api/[controller]")]
    public class OrdersController : Controller
    {
        private readonly OrderService _orderService;

        public OrdersController(OrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            var order = await _orderService.CreateOrder(request);
            return Ok(order);
        }

        /// <summary>
        /// Updates the status of all bookings in an order.
        /// </summary>
        [HttpPatch("{orderId}/status")]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateBookingStatusRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "InvalidRequest",
                    Message = "Status must be provided."
                });
            }

            var success = await _orderService.UpdateOrderStatus(orderId, request.Status);

            if (!success)
            {
                return NotFound(new ErrorResponseDto
                {
                    ErrorCode = "OrderNotFound",
                    Message = "Order not found or status update failed."
                });
            }

            return NoContent(); // No response body needed on success
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(Guid id)
        {
            var order = await _orderService.GetOrder(id);
            if (order == null)
                return NotFound();
            return Ok(order);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrdersByUser([FromQuery] Guid userId)
        {
            var orders = await _orderService.GetOrdersByUser(userId);
            return Ok(orders);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var success = await _orderService.CancelOrder(id);
            if (!success)
                return NotFound();
            return NoContent();
        }


    }
}

