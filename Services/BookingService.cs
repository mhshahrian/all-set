using System.Text.Json;
using AllSet.Domain;
using Microsoft.EntityFrameworkCore;
using static AllSet.DTOs.DataTransferObjects;

namespace AllSet.Services
{
    public class BookingService
    {
        private readonly AllSetDbContext _dbContext;
        private readonly AvailabilityService _availabilityService;

        public BookingService(AllSetDbContext dbContext, AvailabilityService availabilityService)
        {
            _dbContext = dbContext;
            _availabilityService = availabilityService;
        }

        public async Task<BookingCreationResultDto> CreateBookings(CreateBookingRequestDto request)
        {
            var orderId = request.OrderId ?? Guid.NewGuid();
            var newBookings = new List<Booking>();
            var result = new BookingCreationResultDto
            {
                OrderId = orderId,
                Success = true,
                Bookings = new List<BookingResponseDto>(),
                Error = new()
            };

            for (int i = 0; i < request.Bookings.Count; i++)
            {
                var req = request.Bookings[i];

                var startUtc = req.StartDateTime.ToUniversalTime();
                var endUtc = req.EndDateTime.ToUniversalTime();

                if (startUtc >= endUtc)
                {
                    result.Error = new BookingValidationError
                    {
                        Index = i,
                        ErrorCode = BookingErrorCodes.StartAfterEnd
                    };
                    result.Success = false;
                    continue;
                }

                bool isOverlapping = await _dbContext.Bookings.AnyAsync(b =>
                    !b.IsDeleted &&
                    b.Status == "confirmed" &&
                    b.ResourceId == req.ResourceId &&
                    b.StartDateTime < endUtc &&
                    b.EndDateTime > startUtc);

                if (isOverlapping)
                {
                    result.Error = new BookingValidationError
                    {
                        Index = i,
                        ErrorCode = BookingErrorCodes.Overlapping
                    };
                    result.Success = false;
                    continue;
                }

                #region Check WorkingHour

                bool isWithinWorkingHours = await _availabilityService.IsWithinWorkingHours(
                    req.ResourceId,
                    req.StartDateTime,
                    req.EndDateTime
                );

                if (!isWithinWorkingHours)
                {
                    result.Error = new BookingValidationError
                    {
                        Index = i,
                        ErrorCode = BookingErrorCodes.OutOfWorkingHours
                    };
                    result.Success = false;
                    continue;
                }

                #endregion


                var booking = new Booking
                {
                    Id = Guid.NewGuid(),
                    ResourceId = req.ResourceId,
                    StartDateTime = startUtc,
                    EndDateTime = endUtc,
                    OrderId = orderId,
                    Status = "pending",
                    Metadata = req.Metadata.HasValue
                        ? JsonDocument.Parse(req.Metadata.Value.GetRawText())
                        : null
                };

                newBookings.Add(booking);
                result.Bookings.Add(new BookingResponseDto
                {
                    Id = booking.Id,
                    ResourceId = booking.ResourceId,
                    StartDateTime = booking.StartDateTime,
                    EndDateTime = booking.EndDateTime,
                    OrderId = booking.OrderId,
                    Metadata = booking.Metadata
                });
            }

            if (newBookings.Any())
            {
                await _dbContext.Bookings.AddRangeAsync(newBookings);
                await _dbContext.SaveChangesAsync();
            }

            return result;
        }

        public async Task<OrderResponseDto?> GetOrderById(Guid orderId)
        {
            var bookings = await _dbContext.Bookings
                .Where(b => !b.IsDeleted && b.Status == "confirmed" && b.OrderId == orderId)
                .ToListAsync();

            if (!bookings.Any()) return null;

            return new OrderResponseDto
            {
                OrderId = orderId,
                Bookings = bookings.Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    ResourceId = b.ResourceId,
                    StartDateTime = b.StartDateTime,
                    EndDateTime = b.EndDateTime,
                    OrderId = b.OrderId
                }).ToList()
            };
        }

        public async Task<List<BookingResponseDto>> GetBookings(Guid resourceId, DateTime startDate, DateTime endDate)
        {
            var bookings = await _dbContext.Bookings
                .Where(b => !b.IsDeleted &&
                            b.ResourceId == resourceId &&
                            b.Status == "confirmed" &&
                            b.StartDateTime >= startDate.ToUniversalTime() &&
                            b.EndDateTime <= endDate.ToUniversalTime())
                .Select(b => new BookingResponseDto
                {
                    Id = b.Id,
                    ResourceId = b.ResourceId,
                    StartDateTime = b.StartDateTime,
                    EndDateTime = b.EndDateTime,
                    OrderId = b.OrderId,
                    Metadata = b.Metadata
                })
                .ToListAsync();

            return bookings;
        }

        public async Task<bool> DeleteBooking(Guid bookingId)
        {
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null) return false;

            booking.IsDeleted = true;
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UpdateBookingStatus(Guid bookingId, string newStatus)
        {
            // Example of updating status in the database
            var booking = await _dbContext.Bookings.FindAsync(bookingId);
            if (booking == null)
            {
                return false; // Booking not found
            }

            booking.Status = newStatus;
            await _dbContext.SaveChangesAsync();

            return true;
        }

    }
}

