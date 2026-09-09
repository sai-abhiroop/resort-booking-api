using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;

namespace ResortBooking.API.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("/api/v{version:apiversion}/villa")]
    public class VillaController : ControllerBase
    {
        private readonly ApplicationContext _db;
        private readonly IMapper _mapper;
        #region Constructor
        public VillaController(ApplicationContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        #endregion

        #region GetVilla
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<IEnumerable<VillaDetailsDto>>),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>),StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<IEnumerable<VillaDetailsDto>>>> GetVillas()
        {
            var villas = await _db.Villa
                               .AsNoTracking()
                               .ToListAsync();
            var villaDto = _mapper.Map<IEnumerable<VillaDetailsDto>>(villas);
            var response = ApiResponse<IEnumerable<VillaDetailsDto>>.Ok(villaDto,"Villas Retrieved Successfully");
            return Ok(response);
        }
        #endregion

        #region GetById
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<VillaDetailsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> GetVillaById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors:"Villa Id value should be greater than 0"));
            }

            var villa = await _db.Villa.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id);

            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors:$"Villa with Id {id} does not exist"));
            }
            var villaDto = _mapper.Map<VillaDetailsDto>(villa);
            var response = ApiResponse<VillaDetailsDto>.Ok(villaDto, $"Villa with Id: {id} Retrieved Successfully");
            return Ok(response);
        }
        #endregion

        #region Create
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<VillaDetailsDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<VillaDetailsDto>>> CreateVilla(CreateVillaDto villaDto)
        {
            if (villaDto == null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors:"Villa Data is Required"));
            }
            var duplicatevilla=await _db.Villa.FirstOrDefaultAsync(u=>u.Name.ToLower()== villaDto.Name.ToLower());
            if(duplicatevilla != null)
            {
                return Conflict(ApiResponse<Object>.Conflict(errors: $"A villa with Name: {villaDto.Name} already exists"));
            }
            var villa = _mapper.Map<Villa>(villaDto);
            villa.CreatedDate = DateTime.Now;
            await _db.Villa.AddAsync(villa);
            await _db.SaveChangesAsync();
            var response = _mapper.Map<VillaDetailsDto>(villa);
            return CreatedAtAction(
                nameof(GetVillaById),
                new { id = villa.Id },
                ApiResponse<VillaDetailsDto>.CreatedAt(response,"Villa Created Successfully")
                );
        }
        #endregion

        #region Update
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse<Object>>> UpdateVilla(int id,UpdateVillaDto villaDto)
        {
            if (id <= 0)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Villa Id value should be greater than 0"));
            }
            var villa = await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors:$"Villa with Id: {id} not found"));
            }
            var duplicatevilla = await _db.Villa.FirstOrDefaultAsync(u=>u.Name.ToLower()==villaDto.Name.ToLower() && u.Id!=id);
            if (duplicatevilla!=null)
            {
                return Conflict(ApiResponse<Object>.Conflict(errors: $"Villa with Name:{villaDto.Name} already exists"));
            }
            _mapper.Map(villaDto, villa);
            villa.UpdatedDate = DateTime.Now;

            await _db.SaveChangesAsync();

            var response = ApiResponse<Object>.NoContent(message: $"Villa with Id: {id} updated successfully");
            return Ok(response);
        }
        #endregion

        #region Delete
        [HttpDelete("{id:int}")]
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
            var villa =await _db.Villa.FindAsync(id);
            if (villa == null)
            {
                return NotFound(ApiResponse<Object>.NotFound(errors: $"Villa with Id: {id} not found"));
            }
            _db.Villa.Remove(villa);
            await _db.SaveChangesAsync();

            var response=ApiResponse<Object>.NoContent(message:$"Villa with Id: {id} deleted successfully");
            return Ok(response);
        }
        #endregion

        //[HttpGet]
        //public string GetVillaByIdName([FromQuery] int id,[FromHeader]string name)
        //{
        //    return "villa: " + id + " : " + name;
        //}
    }
}