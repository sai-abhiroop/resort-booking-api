using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;

namespace ResortBooking.API.Controllers.v1
{
    [Route("api/v{version:apiversion}/villa-amenities")]
    [ApiVersion("1.0")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        public AmenitiesController(ApplicationContext db,IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AmenitiesDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AmenitiesDetailsDto>>> GetAmenities()
        {
            var amenities=await _db.VillaAmenities
                                .AsNoTracking()
                                .ToListAsync();
            var amenitiesDetails = _mapper.Map<IEnumerable<AmenitiesDetailsDto>>(amenities);
            return Ok(ApiResponse<IEnumerable<AmenitiesDetailsDto>>.Ok(amenitiesDetails,"Villa Amenities retrieved successfully"));
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<AmenitiesDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AmenitiesDetailsDto>> GetAmenitiesById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Id should be greater than 0"));
            }
            var amenity =await _db.VillaAmenities
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(a => a.Id == id);
            if(amenity== null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"There is No amenity with Id: {id}"));
            }
            var amenityDto = _mapper.Map<AmenitiesDetailsDto>(amenity);
            return Ok(ApiResponse<AmenitiesDetailsDto>.Ok(amenityDto,"Villa Amenity Retrieved Successfully"));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AmenitiesDetailsDto>),StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AmenitiesDetailsDto>> CreateVillaAmenity(CreateAmenitiesDto amenitydto)
        {
            if(amenitydto == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Details are required"));
            }
            var isVillaExists = await _db.Villa.FirstOrDefaultAsync(a => a.Id == amenitydto.VillaId);
            if (isVillaExists==null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"A amenity with Villa Id {amenitydto.VillaId} does not exist"));
            }
            var newamenity = _mapper.Map<VillaAmenities>(amenitydto);
            newamenity.CreatedDate=DateTime.Now;
            newamenity.UpdatedDate=DateTime.Now;
            await _db.VillaAmenities.AddAsync(newamenity);
            await _db.SaveChangesAsync();
            var amenitydetails = _mapper.Map<AmenitiesDetailsDto>(newamenity);
            return CreatedAtAction(nameof(GetAmenitiesById), new {id=newamenity.Id} ,ApiResponse<AmenitiesDetailsDto>.CreatedAt(amenitydetails,"Villa Amenity Created Sucessfully"));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateAmenity(int id,UpdateAmenitiesDto amenitydto)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Id value should be greater than 0"));
            }
            if (amenitydto == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Details are required"));
            }
            var amenity=await _db.VillaAmenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Amenity with Id: {id} not found"));
            }
            var isVillaExists = await _db.Villa.FirstOrDefaultAsync(a => a.Id == amenitydto.VillaId);
            if (isVillaExists == null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"A amenity with Villa Id {amenity.VillaId} does not exist"));
            }
            _mapper.Map(amenitydto, amenity);
            amenity.UpdatedDate = DateTime.Now;
            await _db.SaveChangesAsync();
            return Ok(ApiResponse<object>.NoContent());
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveAmenity(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Id value should be greater than 0"));
            }
            var amenity = await _db.VillaAmenities.FindAsync(id);
            if (amenity == null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Amenity with Id: {id} not found"));
            }
            _db.VillaAmenities.Remove(amenity);
            await _db.SaveChangesAsync();

            var response = ApiResponse<object>.NoContent(message: $"Amenity with Id: {id} deleted successfully");
            return Ok(response);
        }
    }
}
