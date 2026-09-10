using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class CreateAmenitiesDto
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        [Range(1, int.MaxValue)]
        public int VillaId { get; set; }
    }
}
