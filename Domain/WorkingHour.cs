namespace AllSet.Domain
{
    public class WorkingHour
    {
        public Guid Id { get; set; }

        public Guid ResourceId { get; set; } // Foreign Key to Resource

        public DayOfWeek DayOfWeek { get; set; } // Monday-Sunday

        public TimeSpan OpenTime { get; set; } // Opening time (e.g., 09:00)

        public TimeSpan CloseTime { get; set; } // Closing time (e.g., 18:00)

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

    }
}

