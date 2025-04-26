using System.Text.Json;

namespace AllSet.DTOs
{
    public class DataTransferObjects
    {
        public class ErrorResponseDto
        {
            public string ErrorCode { get; set; }  // e.g., "InvalidRequest", "ValidationError"

            public string Message { get; set; }    // User-friendly error message

            public object? Details { get; set; }   // Optional: Additional details for debugging
        }

        //

        public class InvalidBookingRequestException : Exception
        {
            public string ErrorCode { get; } = "INVALID_BOOKING_REQUEST";

            public InvalidBookingRequestException(string message) : base(message) { }
        }

        public class BookingConflictException : Exception
        {
            public string ErrorCode { get; } = "BOOKING_CONFLICT";

            public BookingConflictException(string message) : base(message) { }
        }

        //

        public class OrderResponseDto
        {
            public Guid OrderId { get; set; }

            public List<BookingResponseDto> Bookings { get; set; } = new();
        }

        public class CreateResourceRequestDto
        {
            public string Name { get; set; }
        }

        public class BookingResponseDto
        {
            public Guid Id { get; set; }

            public Guid ResourceId { get; set; }

            public DateTime StartDateTime { get; set; }

            public DateTime EndDateTime { get; set; }

            public Guid OrderId { get; set; }

            public JsonDocument? Metadata { get; set; }

        }

        public class CreateBookingRequestDto
        {
            public Guid? OrderId { get; set; } // If null, a new order will be created

            public List<BookingRequestDto> Bookings { get; set; } = new();
        }

        public class BookingRequestDto
        {
            public Guid ResourceId { get; set; }

            public DateTime StartDateTime { get; set; }

            public DateTime EndDateTime { get; set; }

            public JsonElement? Metadata { get; set; }
        }

        public class UpdateBookingStatusRequestDto
        {
            public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"
        }

        public class CreateOrderRequestDto
        {
            public Guid UserId { get; set; }
        }

        public class BookingCreationResultDto
        {
            public bool Success { get; set; }

            public Guid? OrderId { get; set; }

            public List<BookingResponseDto>? Bookings { get; set; }

            public BookingValidationError? Error { get; set; }
        }

        public class BookingValidationError
        {
            public int Index { get; set; } // Index of the failed booking in the request

            public string ErrorCode { get; set; }
        }

        public static class BookingErrorCodes
        {
            public const string InvalidRequest = "InvalidRequest";

            public const string InvalidMetadata = "InvalidMetadata";

            public const string BookingConflict = "BookingConflict";

            public const string OutOfWorkingHours = "OutOfWorkingHours";

            public const string StartAfterEnd = "StartAfterEnd";

            public const string Overlapping = "Overlapping";

            // Add more as needed
        }

    }
}
