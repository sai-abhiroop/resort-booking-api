using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Services
{
    public class VillaService : IVillaService
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;

        public VillaService(ApplicationContext db,IMapper mapper,IImageService imageService)
        {
            _db = db;
            _mapper = mapper;
            _imageService = imageService;
        }

        public async Task<PaginationResultDto<VillaDetailsDto>> GetVillasAsync(string? filterBy, string? filterQuery, string? sortBy, string? sortOrder, int page, int pageSize)
        {
            var villaQuery = _db.Villa.AsNoTracking().AsQueryable();
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
            {
                switch (filterBy.ToLower())
                {
                    case "name":
                        villaQuery = villaQuery.Where(v => v.Name.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "details":
                        villaQuery = villaQuery.Where(v => v.Details!=null && v.Details.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "price":
                        if (double.TryParse(filterQuery, out double price))
                            villaQuery = villaQuery.Where(v => v.Price == price);
                        break;
                    case "minprice":
                        if (double.TryParse(filterQuery, out double minprice))
                            villaQuery = villaQuery.Where(v => v.Price >= minprice);
                        break;
                    case "maxprice":
                        if (double.TryParse(filterQuery, out double maxprice))
                            villaQuery = villaQuery.Where(v => v.Price <= maxprice);
                        break;
                    case "sqft":
                        if (int.TryParse(filterQuery, out int sqft))
                            villaQuery = villaQuery.Where(v => v.Sqft == sqft);
                        break;
                    case "minsqft":
                        if (double.TryParse(filterQuery, out double minsqft))
                            villaQuery = villaQuery.Where(v => v.Sqft >= minsqft);
                        break;
                    case "maxsqft":
                        if (double.TryParse(filterQuery, out double maxsqft))
                            villaQuery = villaQuery.Where(v => v.Sqft <= maxsqft);
                        break;
                    case "occupancy":
                        if (int.TryParse(filterQuery, out int occupancy))
                            villaQuery = villaQuery.Where(v => v.Occupancy == occupancy);
                        break;
                    case "minoccupancy":
                        if (int.TryParse(filterQuery, out int minoccupancy))
                            villaQuery = villaQuery.Where(v => v.Occupancy >= minoccupancy);
                        break;
                    case "maxoccupancy":
                        if (int.TryParse(filterQuery, out int maxoccupancy))
                            villaQuery = villaQuery.Where(v => v.Occupancy <= maxoccupancy);
                        break;
                }

            }
            if (!string.IsNullOrEmpty(sortBy))
            {
                var isDescending = sortOrder?.ToLower() == "desc";
                switch (sortBy.ToLower())
                {
                    case "name":
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Name) : villaQuery.OrderBy(v => v.Name);
                        break;
                    case "price":
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Price) : villaQuery.OrderBy(v => v.Price);
                        break;
                    case "occupancy":
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Occupancy) : villaQuery.OrderBy(v => v.Occupancy);
                        break;
                    case "sqft":
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Sqft) : villaQuery.OrderBy(v => v.Sqft);
                        break;
                    case "id":
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Id) : villaQuery.OrderBy(v => v.Id);
                        break;
                    default:
                        villaQuery = isDescending ? villaQuery.OrderByDescending(v => v.Id) : villaQuery.OrderBy(v => v.Id);
                        break;
                }
            }
            else
            {
                villaQuery = villaQuery.OrderBy(v => v.Id);
            }
            var skip = (page - 1) * pageSize;
            var totalCount = await villaQuery.CountAsync();
            var totalPages = (int)Math.Ceiling(((double)totalCount / pageSize));
            var villas = await villaQuery
                               .Skip(skip)
                               .Take(pageSize)
                               .ToListAsync();
            var villaDtos = _mapper.Map<IEnumerable<VillaDetailsDto>>(villas);
            return new PaginationResultDto<VillaDetailsDto>
            {
                Items = villaDtos,
                CurrentPage = page,
                PageSize = pageSize,
                TotalRecords = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<VillaDetailsDto?> GetVillaByIdAsync(int id)
        {
            var villa = await _db.Villa.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);
            if (villa == null)
            {
                return null;
            }
            return _mapper.Map<VillaDetailsDto>(villa);
        }

        public async Task<VillaDetailsDto?> CreateVillaAsync(CreateVillaDto villaDto)
        {
            var duplicatevilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower());
            if (duplicatevilla != null)
            {
                return null;
            }
            var villa = _mapper.Map<Villa>(villaDto);
            if (villaDto.Image != null)
            {
                if (!_imageService.ValidateImage(villaDto.Image))
                {
                    throw new ArgumentException("Image is in Invalid Format");
                }
                villa.ImageUrl = await _imageService.UploadImageAsync(villaDto.Image);
            }
            villa.CreatedDate = DateTime.UtcNow;
            await _db.Villa.AddAsync(villa);
            await _db.SaveChangesAsync();
            var response = _mapper.Map<VillaDetailsDto>(villa);
            return response;
        }

        public async Task<bool> UpdateVillaAsync(int id,UpdateVillaDto villaDto)
        {
            var villa = await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return false;
            }
            var duplicatevilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower() && u.Id != id);
            if (duplicatevilla != null)
            {
                throw new InvalidOperationException($"Villa with Name:{villaDto.Name} already exists");
            }
            var oldImage = villa.ImageUrl;
            _mapper.Map(villaDto, villa);
            villa.UpdatedDate = DateTime.UtcNow;
            if (villaDto.Image != null)
            {
                if (!_imageService.ValidateImage(villaDto.Image))
                {
                    throw new ArgumentException("Image is in Invalid Format");
                }
                villa.ImageUrl = await _imageService.UploadImageAsync(villaDto.Image);
                if (!string.IsNullOrEmpty(oldImage) && oldImage != villa.ImageUrl)
                {
                    await _imageService.DeleteImageAsync(oldImage);
                }
            }
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteVillaAsync(int id)
        {
            var villa = await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return false;
            }
            var imageUrl = villa.ImageUrl;
            _db.Villa.Remove(villa);
            await _db.SaveChangesAsync();
            if (!string.IsNullOrEmpty(imageUrl))
            {
                await _imageService.DeleteImageAsync(imageUrl);
            }
            return true;
        }
    }
}