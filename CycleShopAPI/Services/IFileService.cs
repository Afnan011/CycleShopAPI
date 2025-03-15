using Microsoft.AspNetCore.Http;

namespace CycleShopAPI.Services
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderPath);
        Task<bool> DeleteFileAsync(string filePath);
        string GetFileUrl(string fileName);
    }
}