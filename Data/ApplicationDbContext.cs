using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Models;

namespace VenueBookingSystem.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Venue> Venues { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Booking> Bookings { get; set; }

        public DbSet<EventType> EventTypes { get; set; }

        // This seeds the "predefined categories" required by the rubric
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventType>().HasData(
                new EventType { EventTypeId = 1, TypeName = "Conference" },
                new EventType { EventTypeId = 2, TypeName = "Concert" },
                new EventType { EventTypeId = 3, TypeName = "Wedding" },
                new EventType { EventTypeId = 4, TypeName = "Workshop" }
            );
        }
    }
}