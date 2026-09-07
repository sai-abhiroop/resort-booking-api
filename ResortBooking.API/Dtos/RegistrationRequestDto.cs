using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class RegistrationRequestDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }

    }
}
