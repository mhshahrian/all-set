using System.Text.Json;
using AllSet.Services;
using Microsoft.AspNetCore.Mvc;
using static AllSet.DTOs.DataTransferObjects;

namespace AllSet.Controllers
{
    [Route("api/[controller]")]
    public class BookingsController : Controller
    {
        private readonly BookingService _bookingService;

        public BookingsController(BookingService bookingService)
        {
            _bookingService = bookingService;
        }

        /// <summary>
        /// Creates a new booking or adds bookings to an existing order.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<BookingCreationResultDto>> CreateBooking([FromBody] CreateBookingRequestDto request)
        {
            if (request == null || request.Bookings == null || !request.Bookings.Any())
            {
                return BadRequest(new BookingCreationResultDto
                {
                    Success = false,
                    Error = new BookingValidationError
                    {
                        Index = -1,
                        ErrorCode = BookingErrorCodes.InvalidRequest
                    }
                });
            }

            for (int i = 0; i < request.Bookings.Count; i++)
            {
                var booking = request.Bookings[i];
                if (booking.Metadata.HasValue && booking.Metadata.Value.ValueKind != JsonValueKind.Object)
                {
                    return UnprocessableEntity(new BookingCreationResultDto
                    {
                        Success = false,
                        Error = new BookingValidationError
                        {
                            Index = i,
                            ErrorCode = BookingErrorCodes.InvalidMetadata
                        }
                    });
                }
            }

            var result = await _bookingService.CreateBookings(request);

            if (!result.Success)
            {
                return Conflict(result); // Includes single error object with ErrorCode and Index
            }

            return CreatedAtAction(nameof(GetOrder), new { orderId = result.OrderId }, result);
        }

        /// <summary>
        /// Retrieves an order with all its bookings.
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrder(Guid orderId)
        {
            var order = await _bookingService.GetOrderById(orderId);
            if (order == null) return NotFound();

            return Ok(order);
        }

        /// <summary>
        /// Retrieves all bookings of a specific resource limited by start, end date.
        /// </summary>
        [HttpGet("/api/resources/{resourceId}/[controller]")]
        public async Task<ActionResult<List<BookingResponseDto>>> GetBookings(
        Guid resourceId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date cannot be after end date.");
            }

            var bookings = await _bookingService.GetBookings(resourceId, startDate, endDate);
            return Ok(bookings);
        }

        ///// <summary>
        ///// Retrieves a specific booking by ID.
        ///// </summary>
        //[HttpGet("{bookingId}")]
        //public async Task<ActionResult<BookingResponseDto>> GetBooking(Guid bookingId)
        //{
        //    var booking = await _bookingService.GetBookingById(bookingId);
        //    if (booking == null) return NotFound();

        //    return Ok(booking);
        //}

        /// <summary>
        /// Deletes a booking.
        /// </summary>
        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> DeleteBooking(Guid bookingId)
        {
            var success = await _bookingService.DeleteBooking(bookingId);
            if (!success) return NotFound("Booking not found.");

            return NoContent();
        }

        /// <summary>
        /// Updates the status of an existing booking.
        /// </summary>
        [HttpPatch("{bookingId}/status")]
        public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, [FromBody] UpdateBookingStatusRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Status))
            {
                return BadRequest(new ErrorResponseDto
                {
                    ErrorCode = "InvalidRequest",
                    Message = "Status must be provided."
                });
            }

            var success = await _bookingService.UpdateBookingStatus(bookingId, request.Status);

            if (!success)
            {
                return NotFound(new ErrorResponseDto
                {
                    ErrorCode = "BookingNotFound",
                    Message = "Booking not found or status update failed."
                });
            }

            return NoContent(); // No content returned on successful update
        }

        ///// <summary>
        ///// Deletes an entire order and all associated bookings.
        ///// </summary>
        //[HttpDelete("order/{orderId}")]
        //public async Task<IActionResult> DeleteOrder(Guid orderId)
        //{
        //    var success = await _bookingService.DeleteOrder(orderId);
        //    if (!success) return NotFound();

        //    return NoContent();
        //}

    }
}

