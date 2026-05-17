using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace TechMove.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileUploadService> _logger;
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB
        private readonly string[] _allowedExtensions = { ".pdf" };
        private readonly string[] _pdfSignatures = { "25504446" }; // %PDF hex

        public FileUploadService(IWebHostEnvironment environment, ILogger<FileUploadService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public bool IsValidPdf(IFormFile file)
        {
            if (file == null || file.Length == 0) return false;
            if (file.Length > _maxFileSize) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension)) return false;

            // Verify PDF signature
            using var stream = file.OpenReadStream();
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            var signature = BitConverter.ToString(buffer).Replace("-", "").ToUpperInvariant();
            
            return _pdfSignatures.Contains(signature);
        }

        public async Task<string> SavePdfAsync(IFormFile file, string folderName)
        {
            if (!IsValidPdf(file))
                throw new ArgumentException("Invalid PDF file");

            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, folderName);
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Saved file: {FileName} to {Path}", file.FileName, filePath);
            return $"/{folderName}/{uniqueFileName}";
        }

        public async Task<byte[]> GetFileBytesAsync(string relativePath)
        {
            var fullPath = Path.Combine(_environment.WebRootPath ?? _environment.ContentRootPath, relativePath.TrimStart('/'));
            
            if (!System.IO.File.Exists(fullPath))
                throw new FileNotFoundException("File not found", relativePath);

            return await System.IO.File.ReadAllBytesAsync(fullPath);
        }
    }
}