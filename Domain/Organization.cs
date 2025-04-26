namespace AllSet.Domain
{
	public class Organization
	{
		public Organization()
		{
		}

		public Guid Id { get; set; } = Guid.NewGuid();

		public string Name { get; set; }

		public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
	}
}

