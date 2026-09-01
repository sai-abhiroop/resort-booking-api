using System.ComponentModel.DataAnnotations;

namespace ResortBooking.API.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? JwtToken {  get; set; }
        public string? RefreshTokenValue {  get; set; }
        public bool IsValid {  get; set; }
        public DateTime ExpiresAt {  get; set; }
    }
}
