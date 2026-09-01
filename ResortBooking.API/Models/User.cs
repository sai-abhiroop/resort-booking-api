using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Name { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        
        [Required]
        public required string password { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Role { get; set; } = "Customer";

        public DateTime CreatedDate { get; set; }

        public DateTime UpdatedDate { get; set; }

    }
}
