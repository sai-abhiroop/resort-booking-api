namespace ResortBooking.API.Services.IServices
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile image);
        Task<bool> DeleteImageAsync(string url);
        bool ValidateImage(IFormFile image);
    }
}
