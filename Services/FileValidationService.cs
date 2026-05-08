using Microsoft.AspNetCore.Http;

namespace TechMove.Services
{
    public class FileValidationService : IFileValidationService
    {
        private readonly string[] _allowedExtensions = { ".pdf" };
        private readonly string[] _pdfSignatures = { "25504446" }; // %PDF in hex

        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Check extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return false;

            // Check file signature (magic number)
            using (var stream = file.OpenReadStream())
            {
                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                var signature = BitConverter.ToString(buffer).Replace("-", "").ToUpperInvariant();
                
                return _pdfSignatures.Contains(signature);
            }
        }

        public string GetFileExtension(IFormFile file)
        {
            return Path.GetExtension(file.FileName).ToLowerInvariant();
        }
    }
}