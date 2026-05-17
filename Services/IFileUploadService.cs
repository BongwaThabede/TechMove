using Microsoft.AspNetCore.Http;

namespace TechMove.Services
{
    public interface IFileUploadService
    {
        Task<string> SavePdfAsync(IFormFile file, string folderName);
        Task<byte[]> GetFileBytesAsync(string relativePath);
        bool IsValidPdf(IFormFile file);
    }
}