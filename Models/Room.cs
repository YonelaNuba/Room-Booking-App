
using System.ComponentModel.DataAnnotations;

namespace RoomBookingSystem.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(1, 1000)]
        public int Capacity { get; set; }

        public string? Location { get; set; }
        public bool IsBoardroom { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<Booking>? Bookings { get; set; }
    }
}
