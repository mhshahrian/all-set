namespace AllSet.Domain
{
    public class Resource
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid OrganizationId { get; set; } // Business that owns it

        public string Name { get; set; } // e.g., "Massage Chair 1"

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }

}

