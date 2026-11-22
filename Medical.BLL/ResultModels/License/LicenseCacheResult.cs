using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseCacheResult
    {
        public bool IsCompleted;
        public string? ReturnUrl { get; set; }
        public string? ModelId { get; set; }
        public string ErrorKey { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public static LicenseCacheResult Success(string returnUrl, string modelId)
        {
            return new LicenseCacheResult
            {
                IsCompleted = true,
                ReturnUrl = returnUrl,
                ModelId = modelId,
            };
        }

        public static LicenseCacheResult Failure(string errorKey, string errorMessage)
        {
            return new LicenseCacheResult
            {
                IsCompleted = false,
                ErrorKey = errorKey,
                ErrorMessage = errorMessage,
            };
        }
    }
}