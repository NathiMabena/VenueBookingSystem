using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;
using VenueBookingSystem.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace VenueBookingSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;
        public EventsController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.Include(e => e.Venue).ToListAsync());
        }
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }
        public IActionResult Create()
        {
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("EventId,EventName,EventDate,Description,VenueId")] Event @event,
            IFormFile? imageFile)
        {
            ModelState.Remove("Venue");
            ModelState.Remove("Bookings");
            ModelState.Remove("ImageUrl");
            if (ModelState.IsValid)
            {
                if (@event.VenueId != null)
                {
                    bool isVenueTaken = await _context.Events.AnyAsync(e =>
                        e.VenueId == @event.VenueId &&
                        e.EventDate.Date == @event.EventDate.Date);
                    if (isVenueTaken)
                    {
                        ModelState.AddModelError("", "Double Booking Prevented: This venue is already hosting another event on that date.");
                        ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                        return View(@event);
                    }
                }
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadedUrl = await _blobService.UploadImageAsync(imageFile);
                        if (string.IsNullOrWhiteSpace(uploadedUrl))
                        {
                            ModelState.AddModelError("", "Image upload did not return a valid URL.");
                            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                            return View(@event);
                        }
                        @event.ImageUrl = uploadedUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                        ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                        return View(@event);
                    }
                }
                try
                {
                    _context.Add(@event);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving event: " + ex.Message);
                }
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events.FindAsync(id);
            if (@event == null) return NotFound();
            if (@event.EventDate < DateTime.Now)
            {
                TempData["ErrorMessage"] = "Historical records are locked. You cannot edit an event that has already passed.";
                return RedirectToAction(nameof(Index));
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("EventId,EventName,EventDate,Description,VenueId")] Event @event,
            IFormFile? imageFile)
        {
            if (id != @event.EventId) return NotFound();
            ModelState.Remove("Venue");
            ModelState.Remove("Bookings");
            ModelState.Remove("ImageUrl");
            if (ModelState.IsValid)
            {
                if (@event.VenueId != null)
                {
                    bool isVenueTaken = await _context.Events.AnyAsync(e =>
                        e.EventId != @event.EventId &&
                        e.VenueId == @event.VenueId &&
                        e.EventDate.Date == @event.EventDate.Date);
                    if (isVenueTaken)
                    {
                        ModelState.AddModelError("", "Double Booking Prevented: This venue is already hosting another event on that date.");
                        ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                        return View(@event);
                    }
                }
                try
                {
                    var existing = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);
                    if (existing == null) return NotFound();
                    existing.EventName = @event.EventName;
                    existing.EventDate = @event.EventDate;
                    existing.Description = @event.Description;
                    existing.VenueId = @event.VenueId;
                    // Keep old event image unless a new valid file is uploaded
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        try
                        {
                            var uploadedUrl = await _blobService.UploadImageAsync(imageFile);
                            if (string.IsNullOrWhiteSpace(uploadedUrl))
                            {
                                ModelState.AddModelError("", "Image upload did not return a valid URL.");
                                ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                                return View(@event);
                            }
                            existing.ImageUrl = uploadedUrl;
                        }
                        catch (Exception ex)
                        {
                            ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
                            return View(@event);
                        }
                    }
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId)) return NotFound();
                    throw;
                }
            }
            ViewData["VenueId"] = new SelectList(_context.Venues, "VenueId", "VenueName", @event.VenueId);
            return View(@event);
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var @event = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null) return NotFound();
            return View(@event);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool hasLinkedBookings = await _context.Bookings.AnyAsync(b => b.EventId == id);
            if (hasLinkedBookings)
            {
                TempData["ErrorMessage"] = "Cannot delete this event because there are active bookings linked to it. Please delete the bookings first.";
                return RedirectToAction(nameof(Index));
            }
            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel != null)
            {
                _context.Events.Remove(eventModel);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        private bool EventExists(int id) => _context.Events.Any(e => e.EventId == id);
    }
}