namespace ResortBooking.API.Models
{
    public class Booking
    {
        public int Id { get; set; }
        public int VillaId { get; set; }
        public Villa Villa { get; set; } = null!;
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int NumberOfGuests { get; set; }
        public double TotalPrice { get; set; }
        public BookingStatus Status { get; set; } = BookingStatus.Confirmed;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public enum BookingStatus
    {
        Cancelled = 0,
        Confirmed = 1
    }
}
