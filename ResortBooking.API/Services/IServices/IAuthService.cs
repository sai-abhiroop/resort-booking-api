using ResortBooking.API.Dtos;

namespace ResortBooking.API.Services.IServices
{
    public interface IAuthService
    {
       Task<UserDto?> RegisterAsync(RegistrationRequestDto registrationRequestDto);  
        
       Task<TokenDto?> LoginAsync(LoginRequestDto loginRequestDto);

       Task<bool> IsEmailExistsAsync(string email);

        Task<TokenDto?> RefreshAccessTokenAsync(RefreshTokenRequestDto refreshTokenRequestDto);
    }
}
