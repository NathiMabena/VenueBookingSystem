using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VenueBookingSystem.Data;
using VenueBookingSystem.Models;
using VenueBookingSystem.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace VenueBookingSystem.Controllers
{
    public class VenuesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BlobService _blobService;

        public VenuesController(ApplicationDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Venues.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
    [Bind("VenueId,VenueName,Location,Capacity")] Venue venue,
    IFormFile? imageFile)
        {
            // Ignore optional/non-posted members during validation
            ModelState.Remove("ImageUrl");
            ModelState.Remove("Events");
            ModelState.Remove("Bookings");
            if (!ModelState.IsValid)
                return View(venue);
            try
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadedUrl = await _blobService.UploadImageAsync(imageFile);
                        if (string.IsNullOrWhiteSpace(uploadedUrl))
                        {
                            ModelState.AddModelError("", "Image upload did not return a valid URL.");
                            return View(venue);
                        }
                        venue.ImageUrl = uploadedUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                        return View(venue);
                    }
                }
                else
                {
                    // No image selected -> use default placeholder
                    venue.ImageUrl = "https://via.placeholder.com/400x200?text=No+Image";
                }
                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving venue: " + ex.Message);
                return View(venue);
            }
        }
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FindAsync(id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
    int id,
    [Bind("VenueId,VenueName,Location,Capacity,ImageUrl")] Venue venue,
    IFormFile? imageFile)
        {
            if (id != venue.VenueId)
                return NotFound();
            // Remove navigation + optional field validation noise
            ModelState.Remove("Events");
            ModelState.Remove("Bookings");
            ModelState.Remove("ImageUrl");
            if (!ModelState.IsValid)
                return View(venue);
            try
            {
                // Always load current row so we can preserve existing image when no new file is uploaded
                var existing = await _context.Venues.FirstOrDefaultAsync(v => v.VenueId == id);
                if (existing == null)
                    return NotFound();
                // Keep old image unless a real new file was uploaded
                if (imageFile != null && imageFile.Length > 0)
                {
                    try
                    {
                        var uploadedUrl = await _blobService.UploadImageAsync(imageFile);
                        if (string.IsNullOrWhiteSpace(uploadedUrl))
                        {
                            ModelState.AddModelError("", "Image upload did not return a valid URL.");
                            return View(venue);
                        }
                        existing.ImageUrl = uploadedUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Image upload failed: " + ex.Message);
                        return View(venue);
                    }
                }
                // Update editable fields
                existing.VenueName = venue.VenueName;
                existing.Location = venue.Location;
                existing.Capacity = venue.Capacity;
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VenueExists(venue.VenueId))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving venue: " + ex.Message);
                return View(venue);
            }
        }
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var venue = await _context.Venues.FirstOrDefaultAsync(m => m.VenueId == id);
            if (venue == null) return NotFound();
            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // 1. THE BOUNCER: Check if any bookings OR events are linked to this venue FIRST
            bool hasLinkedBookings = await _context.Bookings.AnyAsync(b => b.VenueId == id);
            bool hasLinkedEvents = await _context.Events.AnyAsync(e => e.VenueId == id);

            if (hasLinkedBookings || hasLinkedEvents)
            {
                // 2. THE ALERT
                TempData["ErrorMessage"] = "Cannot delete this venue because there are active Events or Bookings linked to it. Please reassign or delete them first.";
                return RedirectToAction(nameof(Index));
            }

            // 3. THE SAFE DELETE
            var venue = await _context.Venues.FindAsync(id);
            if (venue != null)
            {
                _context.Venues.Remove(venue);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id) => _context.Venues.Any(e => e.VenueId == id);
    }
}