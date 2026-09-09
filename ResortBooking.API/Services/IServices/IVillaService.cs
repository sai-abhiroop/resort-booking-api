using ResortBooking.API.Dtos;

namespace ResortBooking.API.Services.IServices
{
    public interface IVillaService
    {
        Task<VillaDetailsDto?> GetVillaByIdAsync(int id);
        Task<PaginationResultDto<VillaDetailsDto>> GetVillasAsync(string? filterBy,string?filterQuery,string? sortBy,string? sortOrder,int page,int pageSize) ;
        Task<VillaDetailsDto?> CreateVillaAsync(CreateVillaDto villaDto);
        Task<bool> UpdateVillaAsync(int id,UpdateVillaDto villaDto);
        Task<bool> DeleteVillaAsync(int id);
    }
}
