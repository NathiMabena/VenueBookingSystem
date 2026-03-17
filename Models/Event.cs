using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueBookingSystem.Models
{
    public class Event
    {
        
        [Key]
        public int EventId { get; set; }

        [Required]
        [StringLength(50)]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        [StringLength (100)]
        public string Description { get; set; }

        //The nullable Foreign Key (This allows an event to exist without a venue yet)
        public int? VenueId { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        //An event can have many bookings
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}
