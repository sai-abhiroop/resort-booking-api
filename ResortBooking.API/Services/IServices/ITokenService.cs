using ResortBooking.API.Models;

namespace ResortBooking.API.Services.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateJwtTokenAsync(ApplicationUser user);
        Task<string> GenerateRefreshTokenAsync();
        Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<(bool IsValid,string? UserId,string? TokenFamilyId,bool IsReused)> ValidateRefreshTokenAsync(string refreshToken);
    }
}
