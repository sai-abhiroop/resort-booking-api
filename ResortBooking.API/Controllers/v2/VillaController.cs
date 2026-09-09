using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;
using System.Diagnostics.Eventing.Reader;
using System.Text;

namespace ResortBooking.API.Controllers.v2
{
    [ApiController]
    [ApiVersion("2.0")]
    [Route("/api/v{version:apiversion}/villa")]
    //[Authorize(Roles="Customer,Admnin")]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IVillaService _villaService;
        #region Constructor
        public VillaController(ApplicationContext db, IMapper mapper,IImageService imageService,IVillaService villaService)
        {
            _db = db;
            _mapper = mapper;
            _imageService = imageService;
            _villaService = villaService;
        }
        #endregion

        #region GetVilla
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDetailsDto>>>> GetVillas([FromQuery]string? filterBy, [FromQuery]string? filterQuery, [FromQuery]string? sortBy, 
            [FromQuery]string? sortOrder = "asc", [FromQuery]int page = 1, [FromQuery]int pageSize=10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
           
            var result = await _villaService.GetVillasAsync(filterBy, filterQuery, sortBy, sortOrder, page, pageSize);
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Successfully Retrieved {result.Items.Count()} Villa(s) ");
            messageBuilder.Append($"(page {result.CurrentPage} of {result.TotalPages},{result.TotalRecords} total records ");
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
                messageBuilder.Append($"Filtered by {filterBy}: '{filterQuery}' ");
            if (!string.IsNullOrEmpty(sortBy))
                messageBuilder.Append($"Sorted by {sortBy}: '{sortOrder?.ToLower() ?? "asc"}')");

            Response.Headers.Append("X-Pagination-CurrentPage", result.CurrentPage.ToString());
            Response.Headers.Append("X-Pagination-PageSize", result.PageSize.ToString());
            Response.Headers.Append("X-Pagination-TotalRecords", result.TotalRecords.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", result.TotalPages.ToString());

            var response = ApiResponse<IEnumerable<VillaDetailsDto>>.Ok(result.Items, messageBuilder.ToString());
            return Ok(response);
        }
        #endregion

        #region GetById
        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> GetVillaById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }

            var villa = await _villaService.GetVillaByIdAsync(id);

            if (villa == null)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Villa with Id {id} does not exist"));
            }
            var villaDto = _mapper.Map<VillaDetailsDto>(villa);
            var response = ApiResponse<VillaDetailsDto>.Ok(villaDto, $"Villa with Id: {id} Retrieved Successfully");
            return Ok(response);
        }
        #endregion

        #region Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<VillaDetailsDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> CreateVilla([FromForm]CreateVillaDto villaDto)
        {
            if (villaDto == null)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Villa Data is Required"));
            }
            var response = await _villaService.CreateVillaAsync(villaDto);
            if (response == null)
            {
                return Conflict(ApiResponse<object>.Conflict(errors: $"A villa with Name: {villaDto.Name} already exists"));
            }
            return CreatedAtAction(
                nameof(GetVillaById),
                new { id = response.Id },
                ApiResponse<VillaDetailsDto>.CreatedAt(response, "Villa Created Successfully")
                );
        }
        #endregion

        #region Update
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> UpdateVilla(int id, [FromForm]UpdateVillaDto villaDto)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }
            try
            {
                var updatedresult = await _villaService.UpdateVillaAsync(id, villaDto);
                if (updatedresult == false)
                {
                    return NotFound(ApiResponse<object>.NotFound(errors: $"Villa with Id: {id} not found"));
                }
                var response = ApiResponse<object>.NoContent(message: $"Villa with Id: {id} updated successfully");
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.Conflict(errors: ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<object>.BadRequest(ex.Message));
            }
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
        //[Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<object>>> RemoveVilla(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }
            var deleteResult = await _villaService.DeleteVillaAsync(id);
            if (deleteResult == false)
            {
                return NotFound(ApiResponse<object>.NotFound(errors: $"Villa with Id: {id} not found"));
            }
            var response = ApiResponse<object>.NoContent(message: $"Villa with Id: {id} deleted successfully");
            return Ok(response);
        }
        #endregion

    }
}