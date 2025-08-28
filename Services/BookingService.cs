
using Microsoft.EntityFrameworkCore;
using RoomBookingSystem.Data;
using RoomBookingSystem.Models;

namespace RoomBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _db;
        public BookingService(BookingDbContext db) => _db = db;

        public async Task<bool> IsRoomAvailableAsync(int roomId, DateTime start, DateTime end, int? excludeBookingId = null)
        {
            if (end <= start) return false;
            var q = _db.Bookings.Where(b => b.RoomId == roomId && b.Status != BookingStatus.Cancelled);
            if (excludeBookingId.HasValue) q = q.Where(b => b.Id != excludeBookingId.Value);
            return !await q.AnyAsync(b => start < b.EndTime && end > b.StartTime);
        }

        public async Task<Booking> CreateAsync(Booking booking)
        {
            if (!await IsRoomAvailableAsync(booking.RoomId, booking.StartTime, booking.EndTime))
                throw new InvalidOperationException("Room is not available.");
            _db.Bookings.Add(booking);
            await _db.SaveChangesAsync();
            return booking;
        }

        public async Task<bool> CancelAsync(int bookingId)
        {
            var b = await _db.Bookings.FindAsync(bookingId);
            if (b == null) return false;
            b.Status = BookingStatus.Cancelled;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RescheduleAsync(int bookingId, DateTime newStart, DateTime newEnd, string? newTitle = null)
        {
            var b = await _db.Bookings.FindAsync(bookingId);
            if (b == null) return false;
            if (!await IsRoomAvailableAsync(b.RoomId, newStart, newEnd, bookingId)) throw new InvalidOperationException("Room not available at new time.");
            b.StartTime = newStart;
            b.EndTime = newEnd;
            if (!string.IsNullOrWhiteSpace(newTitle)) b.Title = newTitle;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task ArchivePastBookingsAsync()
        {
            var now = DateTime.UtcNow;
            var list = await _db.Bookings.Where(b => !b.IsArchived && b.EndTime <= now).ToListAsync();
            foreach (var b in list) b.IsArchived = true;
            if (list.Count>0) await _db.SaveChangesAsync();
        }

        // Very simple recurring generator: for bookings with Recurrence and RecurrenceEnd, create instances for next 30 days (if not overlapping)
        public async Task GenerateRecurringInstancesAsync()
        {
            var templates = await _db.Bookings.Where(b => b.Recurrence!=null && b.RecurrenceEnd!=null).ToListAsync();
            var now = DateTime.UtcNow;
            var horizon = now.AddDays(30);
            foreach (var t in templates)
            {
                var start = t.StartTime.ToLocalTime();
                var end = t.EndTime.ToLocalTime();
                var next = start;
                while (next.ToUniversalTime() <= horizon && (t.RecurrenceEnd==null || next.ToUniversalTime() <= t.RecurrenceEnd.Value))
                {
                    if (next.ToUniversalTime() > now)
                    {
                        // check exists
                        var exists = await _db.Bookings.AnyAsync(b => b.Title==t.Title && b.StartTime==next.ToUniversalTime() && b.RoomId==t.RoomId);
                        if (!exists)
                        {
                            var nb = new Booking {
                                RoomId = t.RoomId,
                                UserId = t.UserId,
                                Title = t.Title,
                                StartTime = next.ToUniversalTime(),
                                EndTime = next.Add(end-start).ToUniversalTime(),
                                Status = BookingStatus.Confirmed
                            };
                            if (await IsRoomAvailableAsync(nb.RoomId, nb.StartTime, nb.EndTime))
                                _db.Bookings.Add(nb);
                        }
                    }

                    if (t.Recurrence=="Daily") next = next.AddDays(1);
                    else if (t.Recurrence=="Weekly") next = next.AddDays(7);
                    else if (t.Recurrence=="Monthly") next = next.AddMonths(1);
                    else break;
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}
