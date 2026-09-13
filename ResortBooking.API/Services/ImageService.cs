using ResortBooking.API.Services.IServices;

namespace ResortBooking.API.Services
{
    public class ImageService : IImageService
    {
        private const long MaxFileSize= 5 * 1024 * 1024;//5Mb
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly IWebHostEnvironment _webHostEnvironment;
        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<bool> DeleteImageAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            var fileName = Path.GetFileName(url);
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas", fileName);
            if (File.Exists(filePath))
            {
                await Task.Run(() => File.Delete(filePath));
                return true;
            }
            return false;
        }

        public async Task<string> UploadImageAsync(IFormFile image)
        {
            if (!ValidateImage(image))
                throw new InvalidOperationException("The file is not a valid image file");
            var uploadFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "villas");
            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }
            var fileExtension = Path.GetExtension(image.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePtah = Path.Combine(uploadFolder, uniqueFileName);
            using (var fileStream = new FileStream(filePtah, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            var imageUrl = $"images/villas/{uniqueFileName}";
            return imageUrl;
        }

        public bool ValidateImage(IFormFile image)
        {
            if(image == null || image.Length == 0)
            {
                return false;
            }
            if(image.Length > MaxFileSize)
            {
                return false;
            }
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return false;
            }
            return true;
        }
    }
}
