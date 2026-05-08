using System.ComponentModel.DataAnnotations;

namespace VenueBookingSystem.Models
{
    public class Venue
    {
        [Key]
        public int VenueId { get; set; }

        [Required]
        [StringLength(80)]
        public string VenueName { get; set; }

        [Required]
        [StringLength(100)]
        public string Location { get; set; }

        [Required]
        [Range(1, 50000, ErrorMessage = "Capacity must be greater than 0.")]
        public int Capacity { get; set; }

        [StringLength(120)]
        public string? ImageUrl { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}