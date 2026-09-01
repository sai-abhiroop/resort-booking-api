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
    public class ResortBookingController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        #region Constructor
        public ResortBookingController(ApplicationContext db, IMapper mapper,IImageService imageService)
        {
            _db = db;
            _mapper = mapper;
            _imageService = imageService;
        }
        #endregion

        #region GetVilla
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDetailsDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDetailsDto>>>> GetVillas([FromQuery]string? filterBy, [FromQuery]string? filterQuery, [FromQuery]string? sortBy, 
            [FromQuery]string? sortOrder = "asc", [FromQuery]int page = 1, [FromQuery]int pageSize=10)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;
            var villaQuery = _db.Villa.AsQueryable();
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
            {
                switch (filterBy.ToLower())
                {
                    case "name":
                        villaQuery = villaQuery.Where(v => v.Name.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "deatils":
                        villaQuery = villaQuery.Where(v => v.Details.ToLower().Contains(filterQuery.ToLower()));
                        break;
                    case "price":
                        if(double.TryParse(filterQuery,out double price))
                            villaQuery = villaQuery.Where(v => v.Price==price);
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
                            villaQuery = villaQuery.Where(v => v.Sqft==sqft);
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
                        villaQuery=isDescending?villaQuery.OrderByDescending(v=>v.Name) :villaQuery.OrderBy(v=>v.Name);
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
            var totalCount = villaQuery.Count();
            var totalPages = (int)Math.Ceiling(((double)totalCount / pageSize));
            var villas = await villaQuery
                               .AsNoTracking()
                               .Skip(skip)
                               .Take(pageSize)
                               .ToListAsync();
            var villaDto = _mapper.Map<IEnumerable<VillaDetailsDto>>(villas);
            var messageBuilder = new StringBuilder();
            messageBuilder.Append($"Successfully Retrievd {villaDto.Count()} Villa(s) ");
            messageBuilder.Append($"(page {page} of {totalPages},{totalCount} toal records ");
            if (!string.IsNullOrEmpty(filterBy) && !string.IsNullOrEmpty(filterQuery))
                messageBuilder.Append($"Filtered by {filterBy}: '{filterQuery}' ");
            if (!string.IsNullOrEmpty(sortBy))
                messageBuilder.Append($"Sorted by {sortBy}: '{sortOrder?.ToLower() ?? "asc"}')");

            Response.Headers.Append("X-Pagination-CurrentPage", page.ToString());
            Response.Headers.Append("X-Pagination-PageSize", pageSize.ToString());
            Response.Headers.Append("X-Pagination-TotalRecords", totalCount.ToString());
            Response.Headers.Append("X-Pagination-TotalPages", totalPages.ToString());

            var response = ApiResponse<IEnumerable<VillaDetailsDto>>.Ok(villaDto, messageBuilder.ToString());
            return Ok(response);
        }
        #endregion

        #region GetById
        [HttpGet("{id:int}")]
        //[AllowAnonymous]
        [ProducesResponseType(typeof(ApiResponse<VillaDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> GetVillaById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }

            var villa = await _db.Villa.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors: $"Villa with Id {id} does not exist"));
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
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> CreateVilla([FromForm]CreateVillaDto villaDto)
        {
            if (villaDto == null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Villa Data is Required"));
            }
            var duplicatevilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower());
            if (duplicatevilla != null)
            {
                return Conflict(ApiResponse<Object>.Conflict(errors: $"A villa with Name: {villaDto.Name} already exists"));
            }
            var villa = _mapper.Map<Villa>(villaDto);
            if(villaDto.Image!= null)
            {
                if (!_imageService.ValidateImage(villaDto.Image))
                {
                    return BadRequest(ApiResponse<Object>.BadRequest("Image is in Invalid Format"));
                }
                villa.ImageUrl=await _imageService.UploadImageAsync(villaDto.Image);
            }
            villa.CreatedDate = DateTime.Now;
            await _db.Villa.AddAsync(villa);
            await _db.SaveChangesAsync();
            var response = _mapper.Map<VillaDetailsDto>(villa);
            return CreatedAtAction(
                nameof(GetVillaById),
                new { id = villa.Id },
                ApiResponse<VillaDetailsDto>.CreatedAt(response, "Villa Created Successfully")
                );
        }
        #endregion

        #region Update
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Object>>> UpdateVilla(int id, [FromForm]UpdateVillaDto villaDto)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }
            var villa = await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors: $"Villa with Id: {id} not found"));
            }
            var duplicatevilla = await _db.Villa.FirstOrDefaultAsync(u => u.Name.ToLower() == villaDto.Name.ToLower() && u.Id != id);
            if (duplicatevilla != null)
            {
                return Conflict(ApiResponse<Object>.Conflict(errors: $"Villa with Name:{villaDto.Name} already exists"));
            }
            if (!_imageService.ValidateImage(villaDto.Image))
            {
                return BadRequest(ApiResponse<Object>.BadRequest("Image is in Invalid Format"));
            }
            var oldImage = villa.ImageUrl;
            _mapper.Map(villaDto, villa);
            villa.UpdatedDate = DateTime.Now;
            if (villaDto.Image != null)
            {
                villa.ImageUrl = await _imageService.UploadImageAsync(villaDto.Image);
                villaDto.ImageUrl=villa.ImageUrl;
                if (!string.IsNullOrEmpty(oldImage) && oldImage != villa.ImageUrl)
                {
                    await _imageService.DeleteImageAsync(oldImage);
                }
            }
            await _db.SaveChangesAsync();

            var response = ApiResponse<Object>.NoContent(message: $"Villa with Id: {id} updated successfully");
            return Ok(response);
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> RemoveVilla(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }
            var villa = await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors: $"Villa with Id: {id} not found"));
            }
            if (!string.IsNullOrEmpty(villa.ImageUrl))
            {
                await _imageService.DeleteImageAsync(villa.ImageUrl);
            }
            _db.Villa.Remove(villa);
            await _db.SaveChangesAsync();

            var response = ApiResponse<Object>.NoContent(message: $"Villa with Id: {id} deleted successfully");
            return Ok(response);
        }
        #endregion

    }
}