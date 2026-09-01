using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ResortBooking.API.Data;
using ResortBooking.API.Models;
using ResortBooking.API.Services.IServices;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ResortBooking.API.Services
{
    public class TokenService : ITokenService
    {
        private readonly ApplicationContext _db;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(ApplicationContext db,IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _db= db;
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
        {
            var key = Encoding.ASCII.GetBytes(_configuration.GetSection("JwtSettings")["Secret"]);
            var roles = await _userManager.GetRolesAsync(user);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),
                    new Claim(ClaimTypes.Name,user.Name),
                    new Claim(ClaimTypes.Email,user.Email),
                    new Claim(ClaimTypes.Role,roles.FirstOrDefault()),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync()
        {
            var randomnumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomnumber);
            var refreshToken = Convert.ToBase64String(randomnumber);
            var exists=await _db.RefreshTokens.AnyAsync(r=>r.RefreshTokenValue==refreshToken);
            if (exists)
            {
                return await GenerateRefreshTokenAsync();
            }
            return refreshToken;
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            var storedToken = await _db.RefreshTokens.FirstOrDefaultAsync(r => r.RefreshTokenValue== refreshToken);
            if (storedToken == null)
            {
                return false;
            }
            storedToken.IsValid = false;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task SaveRefreshTokenAsync(string userId, string jwtTokenId, string refreshToken, DateTime expiresAt)
        {
            var refreshTokenEntity = new RefreshToken()
            {
                UserId = userId,
                JwtToken=jwtTokenId,
                RefreshTokenValue=refreshToken,
                ExpiresAt=expiresAt,
                IsValid = true
            };
            await _db.RefreshTokens.AddAsync(refreshTokenEntity);
            await _db.SaveChangesAsync();
        }

        public async Task<(bool IsValid, string? UserId, string? TokenFamilyId, bool IsReused)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var storedToken=await _db.RefreshTokens.FirstOrDefaultAsync(u=>u.RefreshTokenValue== refreshToken);
            if (storedToken==null)
            {
                return (false, null, null, false);
            }

            //CRITICAL SECURITY CHECK: TOKEN RUSED
            //If token exists but marked as invalid,someone tried to reuse it.
            //This is a strong indicator of token theft
            if (!storedToken.IsValid)
            {
                var tokenFamily = await _db.RefreshTokens.Where(u => u.JwtToken == storedToken.JwtToken && u.UserId == storedToken.UserId).ToListAsync();
                if (tokenFamily.Count > 0)
                {
                    foreach(var token in tokenFamily)
                    {
                        token.IsValid = false;
                    }
                    await _db.SaveChangesAsync();
                }
                return (false, storedToken.UserId, storedToken.JwtToken, true);
            }
            if (storedToken.ExpiresAt < DateTime.UtcNow)
            {
                return (false, null, null, false);
            }
            return (true, storedToken.UserId, storedToken.JwtToken, false);
        }
    }
}
