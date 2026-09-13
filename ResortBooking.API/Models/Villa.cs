using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Models
{
    public class Villa
    {
        public int Id { get; set; }
        [Required]
        public required string Name { get; set; }
        public string? Details {  get; set; }
        public double Price {  get; set; }
        public int Sqft { get; set; }
        public int Occupancy {  get; set; }
        public string? ImageUrl {  get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate {  get; set; }
        public ICollection<VillaAmenities> Amenities { get; set; } = new List<VillaAmenities>();
    }
}
