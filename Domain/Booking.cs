using System.Text.Json;

namespace AllSet.Domain
{
    public class Booking
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrderId { get; set; } // Links to the parent order

        public Guid ResourceId { get; set; } // What is being booked

        public int Quantity { get; set; }

        public JsonDocument? Metadata { get; set; }

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

    }
}