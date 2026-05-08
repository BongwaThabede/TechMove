using Microsoft.AspNetCore.Http;

namespace TechMove.Services
{
    public interface IFileValidationService
    {
        bool IsValidPdf(IFormFile file);
        string GetFileExtension(IFormFile file);
    }
}