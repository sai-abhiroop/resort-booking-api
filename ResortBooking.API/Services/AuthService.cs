using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ResortBooking.API.Configuration;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;

namespace ResortBooking.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtSettings _jwtSettings;
        private readonly IMapper _mapper;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;
        public AuthService(ApplicationContext db, IMapper mapper,UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtOptions, RoleManager<IdentityRole> roleManager, ITokenService tokenService,ILogger<AuthService> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _jwtSettings = jwtOptions.Value;
            _mapper = mapper;
            _tokenService = tokenService;
            _logger = logger;
        }
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(u=>u.Email.ToLower()==email.ToLower());
        }

        public async Task<TokenDto?> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
            if (user == null)
            {
                _logger.LogWarning("Login failed for email {Email}: invalid credentials", loginRequestDto.Email);
                return null;
            }
            var isCorrectPassword=await _userManager.CheckPasswordAsync(user,loginRequestDto.Password);
            if(!isCorrectPassword)
            {
                _logger.LogWarning("Login failed for email {Email}: invalid credentials", loginRequestDto.Email);
                return null;
            } 
            var token =await _tokenService.GenerateJwtTokenAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jwtTokenId =jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var newRefreshToken =await  _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenexpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes);
            await _tokenService.SaveRefreshTokenAsync(user.Id,jwtTokenId,newRefreshToken, refreshTokenexpiryDate);
            _logger.LogInformation("User logged in successfully with UserId {UserId}", user.Id);
            var response= new TokenDto
            {
                AccessToken = token,
                RefreshToken=newRefreshToken,
                ExpiresAt=jwtToken.ValidTo
            };
            return response;
        }

        public async Task<UserDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto)
        {
                if (await IsEmailExistsAsync(registrationRequestDto.Email))
                {
                    throw new InvalidOperationException($"A user with Email :{registrationRequestDto.Email} already Exists");
                }

                var user = new ApplicationUser()
                {
                    Name = registrationRequestDto.Name,
                    Email = registrationRequestDto.Email,
                    UserName=registrationRequestDto.Email,
                    NormalizedEmail=registrationRequestDto.Email.ToUpper(),
                    EmailConfirmed=true
                };
                var result= await _userManager.CreateAsync(user, registrationRequestDto.Password);
                if (!result.Succeeded)
                {
                    var errors = String.Join(",", result.Errors.Select(e => e.Description));
                    throw new ArgumentException($"Registration failed: {errors}");
                }
                const string defaultRole = "Customer";
                if(!await _roleManager.RoleExistsAsync(defaultRole))
                {
                    await _roleManager.CreateAsync(new IdentityRole(defaultRole));
                }
                await _userManager.AddToRoleAsync(user,defaultRole);
                _logger.LogInformation("User registered successfully with UserId {UserId} and Role {Role}", user.Id,defaultRole);
                var userDto= _mapper.Map<UserDto>(user);
                userDto.Role = defaultRole;
                return userDto;
        }

        public async Task<TokenDto?> RefreshAccessTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto)
        {
                if (string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
                    return null;
               //validate Refresh Token
               var (isValid,userId,tokenFamilyId,tokenReused)=await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDto.RefreshToken);
                //Token Reuese Detection
                if (tokenReused)
                {
                _logger.LogWarning("Refresh token reuse detected for UserId {UserId}", userId);
                    return null;
                }
                //Token Inavlid or Expired
                if(!isValid || string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenFamilyId))
                {
                    return null;
                }

                //get user
                var user =await _db.ApplicationUsers.FindAsync(userId);
                if (user == null)
                {
                    return null;
                }

                //revoke old refresh token
                await _tokenService.RevokeRefreshTokenAsync(refreshTokenRequestDto.RefreshToken);
                //generate new Access and refresh token
                var token = await _tokenService.GenerateJwtTokenAsync(user);
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
            
                var newRefreshToken = await _tokenService.GenerateRefreshTokenAsync();
                var refreshTokenexpiryDate = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpirationMinutes);
                await _tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenexpiryDate);
                var response = new TokenDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };
                return response;
        }
    }
}
