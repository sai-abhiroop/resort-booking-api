using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Dtos
{
    public class UpdateVillaDto
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        public string? Details { get; set; }
        [Range(0.01, double.MaxValue)]
        public double Price { get; set; }
        [Range(1, int.MaxValue)]
        public int Sqft { get; set; }
        [Range(1, int.MaxValue)]
        public int Occupancy { get; set; }
        public IFormFile? Image { get; set; }
    }
}
