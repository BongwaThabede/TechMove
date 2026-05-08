using Microsoft.AspNetCore.Http;
using TechMove.Services;
using Xunit;

namespace TechMove.Tests;

public class FileValidationServiceTests
{
    [Fact]
    public void IsValidPdf_ReturnsTrue_ForPdfSignatureAndExtension()
    {
        var service = new FileValidationService();
        var bytes = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D }; // %PDF-
        using var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "agreement.pdf");

        var isValid = service.IsValidPdf(file);

        Assert.True(isValid);
    }

    [Fact]
    public void IsValidPdf_ReturnsFalse_ForExecutableFile()
    {
        var service = new FileValidationService();
        var bytes = new byte[] { 0x4D, 0x5A, 0x90, 0x00 }; // MZ
        using var stream = new MemoryStream(bytes);
        IFormFile file = new FormFile(stream, 0, bytes.Length, "file", "malware.exe");

        var isValid = service.IsValidPdf(file);

        Assert.False(isValid);
    }
}
