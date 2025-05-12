namespace AllSet.Domain
{
    public class WorkingTimeOverride
    {
        public WorkingTimeOverride()
        {
        }

        public Guid Id { get; set; }

        public Guid ResourceId { get; set; }

        public DateTime Date { get; set; }

        public bool IsClosed { get; set; } // true = holiday

        public TimeSpan? OpenTime { get; set; }

        public TimeSpan? CloseTime { get; set; }

        public string? Note { get; set; } // optional: "Christmas", "Late opening", etc.
    }
}

