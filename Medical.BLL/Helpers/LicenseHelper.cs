using Medical.BLL.Constants;
using Medical.BLL.ViewModels.License;
using Microsoft.AspNetCore.Http;

namespace Medical.BLL.Helpers
{
    public static class LicenseHelper
    {
        public static async Task<byte[]> ConvertToBytesAsync(this IFormFile? file)
        {
            if (file == null)
            {
                return [];
            }

            if (file.Length == 0)
            {
                return [];
            }

            byte[] fileBytes = new byte[file.Length];

            await using Stream stream = file.OpenReadStream();
            
            await stream.ReadExactlyAsync(fileBytes, 0, (int)file.Length);

            return fileBytes;
        }

        public static CachedDocumentStatus CheckCachedDocumentStatus(this LicenseCreateViewModel model, byte[]? documentBytes)
        {
            // Missing means it expired (10 minutes),
            // It cannot mean that the file was never uploaded,
            // Because this method is never executed unless (LicenseDocumnet is null && CachedDocumentFileName is not null).
            
            if (documentBytes is null || documentBytes.Length == 0)
            {   
                return CachedDocumentStatus.Expired;
            }
            return CachedDocumentStatus.Exists;
        }      
    }
}