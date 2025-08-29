using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RoomBooking.Data;
using RoomBooking.Models;
using RoomBooking.Services;
using RoomBookingSystem.Data;
using RoomBookingSystem.Models;
using RoomBookingSystem.Services;


namespace RoomBooking.Controllers
{
    [Authorize]
    public class BookingsController : Controller
    {
        private readonly BookingDbContext _db;
        private readonly IBookingService _svc;
        private readonly UserManager<ApplicationUser> _userManager;


        public BookingsController(BookingDbContext db, IBookingService svc, UserManager<ApplicationUser> userManager)
        { _db = db; _svc = svc; _userManager = userManager; }


        // Upcoming/current bookings for the logged-in user
        public async Task<IActionResult> Index()
        {
            var uid = _userManager.GetUserId(User);
            var now = DateTime.UtcNow;
            var list = await _db.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == uid && !b.IsArchived && b.Status != BookingStatus.Cancelled)
            .OrderBy(b => b.StartTime)
            .ToListAsync();
            return View(list);
        }


        public async Task<IActionResult> Previous()
        {
            var uid = _userManager.GetUserId(User);
            var list = await _db.Bookings
            .Include(b => b.Room)
            .Where(b => b.UserId == uid && b.IsArchived)
            .OrderByDescending(b => b.EndTime)
            .ToListAsync();
            return View(list);
        }


        public async Task<IActionResult> Create()
        {
            ViewBag.Rooms = await _db.Rooms.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Booking model)
        {
            ViewBag.Rooms = await _db.Rooms.Where(r => r.IsActive).OrderBy(r => r.Name).ToListAsync();
            if (!ModelState.IsValid) return View(model);


            model.UserId = _userManager.G