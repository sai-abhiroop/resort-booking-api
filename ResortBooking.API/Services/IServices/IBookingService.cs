using ResortBooking.API.Dtos;

namespace ResortBooking.API.Services.IServices
{
    public interface IBookingService
    {
        Task<BookingDetailsDto?> CreateBookingAsync(CreateBookingDto createBookingDto, string userId);
        Task<IEnumerable<BookingDetailsDto>> GetMyBookingsAsync(string userId);
        Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId, string userId, bool isAdmin);
        Task<bool> CancelBookingAsync(int bookingId, string userId, bool isAdmin);
    }
}
