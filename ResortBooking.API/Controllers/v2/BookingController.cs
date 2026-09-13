using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.API.Dtos;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("/api/v{version:apiversion}/booking")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        public BookingController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BookingDetailsDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<BookingDetailsDto>>> CreateBooking(CreateBookingDto createBookingDto)
        {
            if (createBookingDto == null)
            {
                throw new ArgumentException("Booking information is required");
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if(string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Error(StatusCodes.Status401Unauthorized,"User identity could not be determined."));
            }
            var booking =await _bookingService.CreateBookingAsync(createBookingDto, userId);
            return CreatedAtAction(
                  nameof(GetBookingById),
                  new { id = booking.Id },
                  ApiResponse<BookingDetailsDto>.CreatedAt(booking, "Booking created successfully")
                );
        }

        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<BookingDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<IEnumerable<BookingDetailsDto>>>> GetMyBookings()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Error(StatusCodes.Status401Unauthorized, "User identity could not be determined."));
            }
            var bookings=await _bookingService.GetMyBookingsAsync(userId);
            var response = ApiResponse<IEnumerable<BookingDetailsDto>>.Ok(bookings, "Bookings retrieved successfully");
            return Ok(response);
        }

        [HttpGet("{id:int}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BookingDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ApiResponse<BookingDetailsDto>>> GetBookingById(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking Id should be greater than 0");
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Error(StatusCodes.Status401Unauthorized, "User identity could not be determined."));
            }
            var isAdmin = User.IsInRole("Admin");
            var booking = await _bookingService.GetBookingByIdAsync(id, userId, isAdmin);
            var response = ApiResponse<BookingDetailsDto>.Ok(booking, $"Booking with Id {id} retrieved successfully");
            return Ok(response);
        }

        [HttpPut("{id:int}/cancel")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<object>> CancelBooking(int id)
        {
            if (id <= 0)
            {
                throw new ArgumentException("Booking Id should be greater than 0");
            }
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(ApiResponse<object>.Error(StatusCodes.Status401Unauthorized, "User identity could not be determined."));
            }
            var isAdmin = User.IsInRole("Admin");
            await _bookingService.CancelBookingAsync(id, userId, isAdmin);
            return NoContent();
        }
    }
}
