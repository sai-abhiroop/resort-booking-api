using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class UserDto
    {
        public required string? Name { get; set; }
        public required string? Email { get; set; }
        public required string Role { get; set; } = "Customer";

    }
}
