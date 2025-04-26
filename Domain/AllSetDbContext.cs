using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AllSet.Domain
{
    public class AllSetDbContext : IdentityDbContext<AllSetUser>
    {
        public AllSetDbContext(DbContextOptions<AllSetDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Booking> Bookings { get; set; }

        public DbSet<Order> Orders { get; set; }

        public DbSet<Organization> Organizations { get; set; }

        public DbSet<Resource> Resources { get; set; }

        public DbSet<WorkingHour> WorkingHours { get; set; }

    }
}

