using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class CreateAmenitiesDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }
        public string? Description { get; set; }
        [Required]
        public int VillaId { get; set; }
    }
}
