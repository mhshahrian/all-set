namespace AllSet.Domain
{
    public class Order
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid UserId { get; set; } // Customer who made the booking

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public string Status { get; set; } // "Pending", "Confirmed", "Cancelled"


        // Navigation property
        public List<Booking> Bookings { get; set; } = new();
    }
}

