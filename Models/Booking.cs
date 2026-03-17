using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VenueBookingSystem.Models
{
    public class Booking
    {
        public int BookingId { get; set; }
      
        public DateTime BookingDate { get; set; }

        //Foreign Keys in the Booking class are not nullable so there is no need for a "?"

        [Required]
        public int EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event Event { get; set; }

        [Required]
        public int VenueId { get; set; }
        [ForeignKey("VenueId")]
        public virtual Venue Venue { get; set; }
    }
}
