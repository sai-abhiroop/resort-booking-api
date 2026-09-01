using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using ResortBooking.API.Dtos;
using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Controllers
{
    [Route("api/auth")]
    [ApiVersionNeutral]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<UserDto>>> Register(RegistrationRequestDto registrationRequestDto)
        {
            if(registrationRequestDto == null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Registartion Data is required"));
            } 
            if(await _authService.IsEmailExistsAsync(registrationRequestDto.Email))
            {
                return Conflict(ApiResponse<Object>.Conflict(errors: $"A User with Email: {registrationRequestDto.Email} already exists"));
            }
            var user =await _authService.RegisterAsync(registrationRequestDto);
            if (user == null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Registartion Failed"));
            }
            var response = ApiResponse<UserDto>.CreatedAt(user, "User Registerd Sucessfully");
            return CreatedAtAction(nameof(Register),response);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<TokenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ApiResponse<TokenDto>>> Login(LoginRequestDto loginRequestDto)
        {
            if (loginRequestDto == null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Login information is required"));
            }
            var loginResponse=await _authService.LoginAsync(loginRequestDto);
            if(loginResponse== null)
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Login Failed. Please Check your credentials"));
            }
            var response = ApiResponse<TokenDto>.Ok(loginResponse, "User login Successful");
            return Ok(response);
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<Object>), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<ApiResponse<TokenDto>>> RefreshAccessToken(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            if (refreshTokenRequestDto == null || string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
            {
                return BadRequest(ApiResponse<Object>.BadRequest(errors: "Refresh Token is required"));
            }
            var tokenResponse = await _authService.RefreshAccessTokenAsync(refreshTokenRequestDto);
            if(tokenResponse== null)
            {
                return Unauthorized(ApiResponse<object>.Error(401, errors:"Ivalid or Expired Refresh Token"));
            }
            var response = ApiResponse<TokenDto>.Ok(tokenResponse, "Token Refreshed Sucessfully");
            return Ok(response);
        }
    }
}
