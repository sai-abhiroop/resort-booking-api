using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class CreateBookingDto
    {
        [Range(1, int.MaxValue)]
        public int VillaId { get; set; }
        [Required]
        public DateTime CheckInDate { get; set; }
        [Required]
        public DateTime CheckOutDate { get; set; }
        [Range(1, int.MaxValue)]
        public int NumberOfGuests { get; set; }
    }
}
