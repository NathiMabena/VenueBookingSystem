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
        [StringLength(100)]
        public string Description { get; set; }

        public int? VenueId { get; set; }

        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }

        [StringLength(120)]
        public string? ImageUrl { get; set; }

        [Display(Name = "Event Type")]
        public int? EventTypeId { get; set; }
        public EventType? EventType { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}