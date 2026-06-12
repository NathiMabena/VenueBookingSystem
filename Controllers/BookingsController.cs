using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;

namespace VenueBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? eventTypeId, DateTime? startDate, DateTime? endDate, bool? isAvailable)
        {
            // Pass the predefined categories to the View for the dropdown menu
            ViewData["EventTypes"] = new SelectList(_context.EventTypes, "EventTypeId", "TypeName", eventTypeId);

            // Start with the base query, including our new EventType table
            var bookings = _context.Bookings
                .Include(b => b.Event)
                    .ThenInclude(e => e.EventType)
                .Include(b => b.Venue)
                .AsQueryable();

            // 1. Text Search Filter (Booking ID or Event Name)
            if (!string.IsNullOrEmpty(searchString))
            {
                bookings = bookings.Where(b => b.Event.EventName.Contains(searchString) || b.BookingId.ToString() == searchString);
            }

            // 2. Event Type Filter
            if (eventTypeId.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventTypeId == eventTypeId);
            }

            // 3. Date Range Filters
            if (startDate.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventDate <= endDate.Value);
            }

            // 4. Venue Availability Filter
            if (isAvailable.HasValue)
            {
                bookings = bookings.Where(b => b.Venue.IsAvailable == isAvailable.Value);
            }

            // Save the current filter states so the UI remembers what the user selected
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStartDate"] = startDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentEndDate"] = endDate?.ToString("yyyy-MM-dd");
            ViewData["CurrentAvailability"] = isAvailable;

            return View(await bookings.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        public IActionResult Create()
        {
            var upcomingEvents = _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventName)
                .ToList();
            ViewData["EventId"] = new SelectList(upcomingEvents, "EventId", "EventName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,BookingDate,EventId,VenueId")] Booking booking)
        {
            ModelState.Remove("Event");
            ModelState.Remove("Venue");
            ModelState.Remove("VenueId");

            if (ModelState.IsValid)
            {
                var linkedEvent = await _context.Events.FindAsync(booking.EventId);

                if (linkedEvent != null)
                {
                    if (linkedEvent.EventDate < DateTime.Now)
                    {
                        ModelState.AddModelError("", "Action Denied: You cannot create a booking for an event that has already passed.");
                    }
                    else if (!linkedEvent.VenueId.HasValue)
                    {
                        ModelState.AddModelError("", "Cannot book '" + linkedEvent.EventName + "' because it does not have a venue assigned yet.");
                    }
                    else
                    {
                        // Double booking check
                        bool isDuplicate = await _context.Bookings.AnyAsync(b =>
                            b.VenueId == linkedEvent.VenueId.Value &&
                            b.Event.EventDate.Date == linkedEvent.EventDate.Date);

                        if (isDuplicate)
                        {
                            ModelState.AddModelError("", "This venue is already booked on that date. Please choose a different event or venue.");
                        }
                        else
                        {
                            booking.VenueId = linkedEvent.VenueId.Value;
                            booking.BookingDate = DateTime.Now;
                            _context.Add(booking);
                            await _context.SaveChangesAsync();
                            return RedirectToAction(nameof(Index));
                        }
                    }
                }
            }

            var upcomingEvents = _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventName)
                .ToList();
            ViewData["EventId"] = new SelectList(upcomingEvents, "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null) return NotFound();
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,EventId,VenueId,BookingDate")] Booking booking)
        {
            if (id != booking.BookingId) return NotFound();
            ModelState.Remove("Event");
            ModelState.Remove("Venue");
            ModelState.Remove("VenueId");

            if (ModelState.IsValid)
            {
                try
                {
                    var linkedEvent = await _context.Events.FindAsync(booking.EventId);
                    if (linkedEvent != null)
                    {
                        if (linkedEvent.EventDate < DateTime.Now)
                        {
                            ModelState.AddModelError("", "Action Denied: You cannot switch to an event that has already passed.");
                            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                            return View(booking);
                        }
                        if (!linkedEvent.VenueId.HasValue)
                        {
                            ModelState.AddModelError("", "Cannot switch to this event because it does not have a venue assigned yet.");
                            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                            return View(booking);
                        }

                        // NEW: Double booking check for EDITS
                        bool isDuplicate = await _context.Bookings.AnyAsync(b =>
                            b.BookingId != booking.BookingId && // CRUCIAL: Ignore the current booking!
                            b.VenueId == linkedEvent.VenueId.Value &&
                            b.Event.EventDate.Date == linkedEvent.EventDate.Date);

                        if (isDuplicate)
                        {
                            ModelState.AddModelError("", "Double Booking Prevented: This venue is already booked on that date.");
                            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                            return View(booking);
                        }

                        // If it passes all checks, assign the venue and update!
                        booking.VenueId = linkedEvent.VenueId.Value;
                    }
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingId)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            return View(booking);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id) => _context.Bookings.Any(e => e.BookingId == id);
    }
}