using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Services
{
    public class AmenitiesService : IAmenitiesService
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        private readonly ILogger<AmenitiesService> _logger;
        public AmenitiesService(ApplicationContext db,IMapper mapper,ILogger<AmenitiesService> logger)
        {
            _db = db;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AmenitiesDetailsDto?> CreateVillaAmenityAsync(CreateAmenitiesDto amenityDto)
        {
            var isVillaExists = await _db.Villa.FirstOrDefaultAsync(a => a.Id == amenityDto.VillaId);
            if (isVillaExists == null)
            {
                return null;
            }
            var newamenity = _mapper.Map<VillaAmenities>(amenityDto);
            newamenity.CreatedDate = DateTime.UtcNow;
            newamenity.UpdatedDate = DateTime.UtcNow;
            await _db.VillaAmenities.AddAsync(newamenity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Amenity created successfully with Id {Id} fo villaId {villaId}", newamenity.Id, newamenity.VillaId);
            return _mapper.Map<AmenitiesDetailsDto>(newamenity);
        }

        public async Task<AmenitiesDetailsDto?> GetAmenitiesByIdAsync(int id)
        {
            var amenity = await _db.VillaAmenities
                                   .AsNoTracking()
                                   .Include(a => a.Villa)
                                   .FirstOrDefaultAsync(a => a.Id == id);
            if (amenity == null)
            {
                return null;
            }
            return _mapper.Map<AmenitiesDetailsDto>(amenity);
        }

        public async Task<IEnumerable<AmenitiesDetailsDto>> GetAmenitiesDetailsAsync()
        {
            var amenities = await _db.VillaAmenities
                                .AsNoTracking()
                                .Include(a => a.Villa)
                                .ToListAsync();
            return _mapper.Map<IEnumerable<AmenitiesDetailsDto>>(amenities);
        }

        public async Task<bool> RemoveAmenityAsync(int id)
        {
            var amenity = await _db.VillaAmenities.FindAsync(id);
            if (amenity == null)
            {
                return false;
            }
            _db.VillaAmenities.Remove(amenity);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Amenity with Id {Id} deleted successfully", amenity.Id);
            return true;
        }

        public async Task<bool> UpdateAmenityAsync(int id, UpdateAmenitiesDto amenityDto)
        {
            var amenity = await _db.VillaAmenities.FindAsync(id);
            if(amenity == null)
            {
                return false;
            }
            var isVillaExists = await _db.Villa.FirstOrDefaultAsync(a => a.Id == amenityDto.VillaId);
            if (isVillaExists == null)
            {
                throw new KeyNotFoundException($"A Villa with Id {amenityDto.VillaId} does not exist");
            }
            _mapper.Map(amenityDto, amenity);
            amenity.UpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            _logger.LogInformation("Amenity with Id {Id} updated successfully", amenity.Id);
            return true;
        }
    }
}
