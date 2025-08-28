
using RoomBookingSystem.Models;

namespace RoomBookingSystem.Services
{
    public interface IBookingService
    {
        Task<bool> IsRoomAvailableAsync(int roomId, DateTime start, DateTime end, int? excludeBookingId = null);
        Task<Booking> CreateAsync(Booking booking);
        Task<bool> CancelAsync(int bookingId);
        Task<bool> RescheduleAsync(int bookingId, DateTime newStart, DateTime newEnd, string? newTitle = null);
        Task ArchivePastBookingsAsync();
        Task GenerateRecurringInstancesAsync();
    }
}
