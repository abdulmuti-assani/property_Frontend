using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using RealEstate.Application.Common.Settings;
using RealEstate.Application.Interfaces;

namespace RealEstate.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private readonly IWebHostEnvironment _environment;
    private readonly FileStorageSettings _settings;

    public LocalFileStorageService(IWebHostEnvironment environment, IOptions<FileStorageSettings> settings)
    {
        _environment = environment;
        _settings = settings.Value;
    }

    public async Task<string> SaveAsync(IFormFile file, string subFolder)
    {
        if (file is null || file.Length == 0)
            throw new InvalidOperationException("Uploaded file is empty.");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException("Uploaded file exceeds the 5 MB limit.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Only .jpg, .jpeg, .png and .webp images are allowed.");

        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var targetDir = Path.Combine(webRoot, _settings.UploadsFolder, subFolder);
        Directory.CreateDirectory(targetDir);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var physicalPath = Path.Combine(targetDir, fileName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return $"{_settings.PublicBaseUrl.TrimEnd('/')}/{_settings.UploadsFolder}/{subFolder}/{fileName}";
    }

    public void Delete(string? fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            return;

        var marker = $"/{_settings.UploadsFolder}/";
        var index = fileUrl.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (index < 0)
            return;

        var relativePath = fileUrl[(index + 1)..].Replace('/', Path.DirectorySeparatorChar);
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var physicalPath = Path.Combine(webRoot, relativePath);

        if (File.Exists(physicalPath))
            File.Delete(physicalPath);
    }
}
