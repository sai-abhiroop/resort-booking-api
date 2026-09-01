using Microsoft.AspNetCore.Identity;

namespace ResortBooking.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
