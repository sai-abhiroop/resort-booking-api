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

        public AmenitiesService(ApplicationContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<AmenitiesDetailsDto?> CreateVillaAmenityAsync(CreateAmenitiesDto amenitydto)
        {
            var isVillaExists = await _db.Villa.FirstOrDefaultAsync(a => a.Id == amenitydto.VillaId);
            if (isVillaExists == null)
            {
                return null;
            }
            var newamenity = _mapper.Map<VillaAmenities>(amenitydto);
            newamenity.CreatedDate = DateTime.UtcNow;
            newamenity.UpdatedDate = DateTime.UtcNow;
            await _db.VillaAmenities.AddAsync(newamenity);
            await _db.SaveChangesAsync();
            return _mapper.Map<AmenitiesDetailsDto>(newamenity);
        }

        public async Task<AmenitiesDetailsDto?> GetAmenitiesByIdAsync(int id)
        {
            var amenity = await _db.VillaAmenities
                                                 .AsNoTracking()
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
                throw new InvalidOperationException($"A Villa with Id {amenity.VillaId} does not exist");
            }
            _mapper.Map(amenityDto, amenity);
            amenity.UpdatedDate = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}
