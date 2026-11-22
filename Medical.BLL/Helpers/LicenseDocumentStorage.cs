using Microsoft.AspNetCore.Http;

namespace Medical.BLL.Helpers
{
    public static class LicenseDocumentStorage
    {
        public static async Task<string> WriteFileAsync(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);

            string fileName = $"{Guid.NewGuid()}{extension}";

            string filePath = Path.Combine("wwwroot", "uploads", fileName);

            using var stream = new FileStream(filePath, FileMode.Create);

            await file.CopyToAsync(stream);

            return fileName;
        }

        public static async Task<string> WriteBytesAsync(byte[] data, string originalFileName)
        {
            string extension = Path.GetExtension(originalFileName);

            string fileName = $"{Guid.NewGuid()}{extension}";
            
            string filePath = Path.Combine("wwwroot", "uploads", fileName);

            await File.WriteAllBytesAsync(filePath, data);

            return fileName;
        }
    
        public static Task DeleteFileAsync(string fileName)
        {
            string filePath = Path.Combine("wwwroot", "uploads", fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            return Task.CompletedTask;
        }
    }
}