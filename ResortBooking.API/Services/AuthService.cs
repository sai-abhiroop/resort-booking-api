using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using ResortBooking.API.Data;
using ResortBooking.API.Dtos;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ResortBooking.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        public AuthService(ApplicationContext db, IMapper mapper,IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ITokenService tokenService)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _mapper = mapper;
            _configuration = configuration;
            _tokenService = tokenService;
        }
        public async Task<bool> IsEmailExistsAsync(string email)
        {
            return await _db.ApplicationUsers.AnyAsync(u=>u.Email.ToLower()==email.ToLower());
        }

        public async Task<TokenDto> LoginAsync(LoginRequestDto loginRequestDto)
        {
            var user = await _userManager.FindByEmailAsync(loginRequestDto.Email);
            var isCorrectPassword=await _userManager.CheckPasswordAsync(user,loginRequestDto.Password);
            if(user ==null || !isCorrectPassword)
            {
                return null;
            } 
            var token =await _tokenService.GenerateJwtTokenAsync(user);
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            var jwtTokenId =jwtToken.Claims.FirstOrDefault(u => u.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var newRefreshToken =await  _tokenService.GenerateRefreshTokenAsync();
            var refreshTokenexpiryDate = DateTime.UtcNow.AddMinutes(5);
            await _tokenService.SaveRefreshTokenAsync(user.Id,jwtTokenId,newRefreshToken, refreshTokenexpiryDate);
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
            try
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
                var result= await _userManager.CreateAsync(user, registrationRequestDto.password);
                if (!result.Succeeded)
                {
                    var errors = String.Join(",", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Registration Failed: {errors}");
                }
                var role = String.IsNullOrEmpty(registrationRequestDto.Role) ? "Customer" : registrationRequestDto.Role;
                if(!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }
                await _userManager.AddToRoleAsync(user,role);

                var userDto= _mapper.Map<UserDto>(user);
                userDto.Role = role;
                return userDto;
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during user registration", ex);
            }
        }

        public async Task<TokenDto?> RefreshAccessTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshTokenRequestDto.RefreshToken))
                    return null;
               //validate Refresh Token
               var (isValid,userId,tokenFamilyId,tokenReused)=await _tokenService.ValidateRefreshTokenAsync(refreshTokenRequestDto.RefreshToken);
                //Token Reuese Detection
                if (tokenReused)
                {
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
                var refreshTokenexpiryDate = DateTime.UtcNow.AddMinutes(5);
                await _tokenService.SaveRefreshTokenAsync(user.Id, tokenFamilyId, newRefreshToken, refreshTokenexpiryDate);
                var response = new TokenDto
                {
                    AccessToken = token,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = jwtToken.ValidTo
                };
                return response;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An unexpected error occured during Access Token Refresh", ex);
            }
        }
    }
}
