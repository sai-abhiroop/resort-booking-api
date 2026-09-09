using ResortBooking.API.Dtos;

namespace ResortBooking.API.Services.IServices
{
    public interface IAmenitiesService
    {
        public Task<IEnumerable<AmenitiesDetailsDto>> GetAmenitiesDetailsAsync();
        public Task<AmenitiesDetailsDto?> GetAmenitiesByIdAsync(int id);
        public Task<AmenitiesDetailsDto?> CreateVillaAmenityAsync(CreateAmenitiesDto amenitydto);
        Task<bool> UpdateAmenityAsync(int id, UpdateAmenitiesDto amenityDto);
        Task<bool> RemoveAmenityAsync(int id);
    }
}
