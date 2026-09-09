using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Models;
using ResortBooking.API.Services;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Data
{
    public static class DataExtensions
    {
        public static void RegisterDbContext(this WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnectionString");

            builder.Services.AddSqlServer<ApplicationContext>(connectionString, optionsAction: options => options.UseSeeding((context,_) =>
            {
                if (!context.Set<Villa>().Any())
                {
                    context.Set<Villa>().AddRange(
                        new Villa
                        {
                            Name = "Royal Villa",
                            Details = "Luxurious villa with stunning ocean views and private beach access.",
                            Price = 500.0,
                            Sqft = 2500,
                            Occupancy = 6,
                            ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa1.jpg",
                            CreatedDate = new DateTime(2024, 1, 1),
                            UpdatedDate = new DateTime(2024, 1, 1)
                        },
                        new Villa
                        {
                            Name = "Diamond Villa",
                            Details = "Elegant villa with marble interiors and panoramic mountain views.",
                            Price = 750.0,
                            Sqft = 3200,
                            Occupancy = 8,
                            ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa2.jpg",
                            CreatedDate = new DateTime(2024, 1, 15),
                            UpdatedDate = new DateTime(2024, 1, 15)
                        },
                        new Villa
                        {
                            Name = "Pool Villa",
                            Details = "Modern villa featuring an infinity pool and outdoor entertainment area.",
                            Price = 350.0,
                            Sqft = 1800,
                            Occupancy = 4,
                            ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa3.jpg",
                            CreatedDate = new DateTime(2024, 2, 1),
                            UpdatedDate = new DateTime(2024, 2, 1)
                        },
                        new Villa
                        {
                            Name = "Luxury Villa",
                            Details = "Premium villa with spa facilities and concierge services.",
                            Price = 900.0,
                            Sqft = 4000,
                            Occupancy = 10,
                            ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa4.jpg",
                            CreatedDate = new DateTime(2024, 2, 14),
                            UpdatedDate = new DateTime(2024, 2, 14)
                        },
                        new Villa
                        {
                            Name = "Garden Villa",
                            Details = "Charming villa surrounded by tropical gardens and nature trails.",
                            Price = 275.0,
                            Sqft = 1500,
                            Occupancy = 3,
                            ImageUrl = "https://dotnetmasteryimages.blob.core.windows.net/bluevillaimages/villa5.jpg",
                            CreatedDate = new DateTime(2024, 3, 1),
                            UpdatedDate = new DateTime(2024, 3, 1)
                        }
                     );
                    context.SaveChanges();
                }
            }));
        }

        public static void RegisterAuthService(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IAuthService, AuthService>();
        }

        public static void RegisterImageService(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IImageService, ImageService>();
        }

        public static void RegisterTokenService(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<ITokenService,TokenService>();
        }

        public static void RegisterVillaService(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IVillaService, VillaService>();
        }

        public static void MigrateDb(this WebApplication app)
        {
            using(var scope = app.Services.CreateScope())
            {
                var dbConteext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
                dbConteext.Database.Migrate();
            }
        }
    }
}
