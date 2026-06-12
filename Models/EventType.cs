using System.ComponentModel.DataAnnotations;

namespace VenueBookingSystem.Models
{
    public class EventType
    {
        [Key]
        public int EventTypeId { get; set; }

        [Required]
        [Display(Name = "Event Type")]
        public string TypeName { get; set; }
    }
}
