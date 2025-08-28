
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RoomBookingSystem.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int RoomId { get; set; }
        public Room? Room { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [DataType(DataType.DateTime)]
        public DateTime StartTime { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime EndTime { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public bool IsArchived { get; set; } = false;

        // Recurrence: none/daily/weekly/monthly
        public string? Recurrence { get; set; }
        public DateTime? RecurrenceEnd { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
