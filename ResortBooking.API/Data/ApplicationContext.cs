using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Models;

namespace ResortBooking.API.Data
{
    public class ApplicationContext(DbContextOptions options):IdentityDbContext(options)
    {
        public DbSet<Villa> Villa{ get; set; }

        public DbSet<User> User { get; set; }

        public DbSet<VillaAmenities> VillaAmenities { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public DbSet<Booking> Bookings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Booking>()
                   .HasOne<Villa>(b => b.Villa)
                   .WithMany()
                   .HasForeignKey(b => b.VillaId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Booking>()
                   .HasOne<ApplicationUser>(b => b.User)
                   .WithMany()
                   .HasForeignKey(b => b.UserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
    
}
