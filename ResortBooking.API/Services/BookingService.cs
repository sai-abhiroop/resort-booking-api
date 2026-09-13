using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;
using System.Data;
namespace ResortBooking.API.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<BookingService> _logger;

        public BookingService(ApplicationContext db, IMapper mapper, ILogger<BookingService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }
        
        public async Task<bool> CancelBookingAsync(int bookingId, string userId, bool isAdmin)
        {
            if (bookingId <= 0)
            {
                throw new ArgumentException("BookingId must be greater than zero");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be empty");
            }
            var booking= await _db.Bookings
                                  .FindAsync(bookingId);
            if(booking==null || (!isAdmin && booking.UserId != userId))
            {
                throw new KeyNotFoundException("Booking Not Found");
            }
            if (booking.Status == BookingStatus.Cancelled)
            {
                throw new InvalidOperationException("Booking is already Cancelled");
            }
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedDate=DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Booking cancelled successfully with Id {BookingId} by UserId {UserId}",bookingId, userId);
            return true;
        }

        public async Task<BookingDetailsDto?> CreateBookingAsync(CreateBookingDto createBookingDto, string userId)
        {
            var villa = await _db.Villa.FindAsync(createBookingDto.VillaId);
            if (villa==null)
            {
                throw new KeyNotFoundException("Villa Not Found");
            }
            if(createBookingDto.CheckInDate.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Check-in date cannot be in the past");
            }
            if(createBookingDto.CheckOutDate.Date <= createBookingDto.CheckInDate.Date)
            {
                throw new ArgumentException("Check-out date must be after check-in date");
            }
            if (createBookingDto.NumberOfGuests > villa.Occupancy)
            {
                throw new ArgumentException("Number of guests exceeds the villa occupancy");
            }
            await using var transaction=await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                var existingBooking = await _db.Bookings
                                        .AnyAsync(b => b.VillaId == createBookingDto.VillaId &&
                                                               b.Status == BookingStatus.Confirmed &&
                                                               createBookingDto.CheckInDate < b.CheckOutDate &&
                                                               createBookingDto.CheckOutDate > b.CheckInDate
                                                  );
                if (existingBooking)
                {
                    throw new InvalidOperationException("Villa is already booked for the selected dates");
                }
                var numberOfNights = (createBookingDto.CheckOutDate - createBookingDto.CheckInDate).Days;
                var totalPrice = numberOfNights * (villa.Price);
                var booking = _mapper.Map<Booking>(createBookingDto);
                booking.UserId = userId;
                booking.TotalPrice = totalPrice;
                booking.Status = BookingStatus.Confirmed;
                booking.CreatedDate = DateTime.UtcNow;
                _db.Bookings.Add(booking);
                await _db.SaveChangesAsync();
                await transaction.CommitAsync();
                booking = await _db.Bookings
                         .Include(b => b.Villa)
                         .Include(b => b.User)
                         .FirstAsync(b => b.Id == booking.Id);

                _logger.LogInformation("Booking created successfully with Id {BookingId} for VillaId {VillaId} and UserId {UserId}", booking.Id, booking.VillaId, booking.UserId);
                return _mapper.Map<BookingDetailsDto>(booking);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<BookingDetailsDto?> GetBookingByIdAsync(int bookingId, string userId, bool isAdmin)
        {
            if (bookingId <= 0)
            {
                throw new ArgumentException("BookingId must be greater than zero");
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be empty");
            }
            var booking=await _db.Bookings
                                 .AsNoTracking()
                                 .Include(b => b.Villa)
                                 .Include(b => b.User)
                                 .FirstOrDefaultAsync(b=>b.Id==bookingId);
            if (booking == null || (!isAdmin && booking.UserId!=userId))
            {
                throw new KeyNotFoundException("Booking not found");
            }
            return _mapper.Map<BookingDetailsDto>(booking);
        }

        public async Task<IEnumerable<BookingDetailsDto>> GetMyBookingsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("UserId cannot be empty");
            }
            var bookings = await _db.Bookings
                                    .AsNoTracking()
                                    .Include(b => b.Villa)
                                    .Include(b => b.User)
                                    .Where(b => b.UserId == userId)
                                    .OrderByDescending(b => b.CreatedDate)
                                    .ToListAsync();
            return _mapper.Map<IEnumerable<BookingDetailsDto>>(bookings);
        }
    }
}
