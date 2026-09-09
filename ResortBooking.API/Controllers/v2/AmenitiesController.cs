using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Controllers.v2
{
    [Route("api/v{version:apiversion}/villa-amenities")]
    [ApiVersion("2.0")]
    [ApiController]
    public class AmenitiesController : ControllerBase
    {
        private readonly IAmenitiesService _amenitiesService;
        public AmenitiesController(IAmenitiesService amenitiesService)
        {
            _amenitiesService = amenitiesService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<AmenitiesDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<AmenitiesDetailsDto>>> GetAmenities()
        {
            var amenitiesDetails =await _amenitiesService.GetAmenitiesDetailsAsync();
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
            var amenityDto =await _amenitiesService.GetAmenitiesByIdAsync(id);
            if (amenityDto== null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"There is No amenity with Id: {id}"));
            }
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
            var amenityDetails =await _amenitiesService.CreateVillaAmenityAsync(amenitydto);
            if (amenityDetails == null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"A Villa with Id {amenitydto.VillaId} does not exist"));
            }
            return CreatedAtAction(nameof(GetAmenitiesById), new {id=amenityDetails.Id} ,ApiResponse<AmenitiesDetailsDto>.CreatedAt(amenityDetails,"Villa Amenity Created Successfully"));
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateAmenity(int id,UpdateAmenitiesDto amenityDto)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Id value should be greater than 0"));
            }
            if (amenityDto == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Amenity Details are required"));
            }
            var updated=await _amenitiesService.UpdateAmenityAsync(id,amenityDto);
            if (updated == false)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Amenity with Id: {id} not found"));
            }
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
            var deleted = await _amenitiesService.RemoveAmenityAsync(id);
            if (!deleted)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Amenity with Id: {id} not found"));
            }
            var response = ApiResponse<object>.NoContent(message: $"Amenity with Id: {id} deleted successfully");
            return Ok(response);
        }
    }
}