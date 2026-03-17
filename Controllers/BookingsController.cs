using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Venue);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            // Fix: Get ALL future events, regardless of whether they have a venue yet
            // This ensures the dropdown is consistent.
            var upcomingEvents = _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventName)
                .ToList();

            ViewData["EventId"] = new SelectList(upcomingEvents, "EventId", "EventName");
            return View();
        }

        // POST: Bookings/Create
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
                    // 1. Check if the event has passed
                    if (linkedEvent.EventDate < DateTime.Now)
                    {
                        ModelState.AddModelError("", "Action Denied: You cannot create a booking for an event that has already passed.");
                    }
                    // 2. Check if the Event actually has a venue assigned
                    else if (!linkedEvent.VenueId.HasValue)
                    {
                        ModelState.AddModelError("", "Cannot book '" + linkedEvent.EventName + "' because it does not have a venue assigned yet. Please update the event first.");
                    }
                    else
                    {
                        // All checks passed!
                        booking.VenueId = linkedEvent.VenueId.Value;
                        booking.BookingDate = DateTime.Now;

                        _context.Add(booking);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                }
            }

            // If we are here, something failed. 
            // IMPORTANT: Reload the EXACT SAME list as the GET method so the dropdown doesn't change!
            var upcomingEvents = _context.Events
                .Where(e => e.EventDate >= DateTime.Now)
                .OrderBy(e => e.EventName)
                .ToList();

            ViewData["EventId"] = new SelectList(upcomingEvents, "EventId", "EventName", booking.EventId);
            return View(booking);
        }
        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
            return View(booking);
        }

        // POST: Bookings/Edit/5
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
                        // NEW CHECK: Prevent saving if the event is in the past!
                        if (linkedEvent.EventDate < DateTime.Now)
                        {
                            ModelState.AddModelError("", "Action Denied: You cannot switch to an event that has already passed.");
                            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                            return View(booking);
                        }

                        // Check if the Event actually has a venue assigned!
                        if (linkedEvent.VenueId.HasValue)
                        {
                            booking.VenueId = linkedEvent.VenueId.Value;
                        }
                        else
                        {
                            ModelState.AddModelError("", "Cannot switch to this event because it does not have a venue assigned yet.");
                            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "EventName", booking.EventId);
                            return View(booking);
                        }
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

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}