using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseRefreshCacheExpiryResult
    {
        public bool IsCompleted;
        public string ErrorKey { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }

        public static LicenseRefreshCacheExpiryResult Success()
        {
            return new LicenseRefreshCacheExpiryResult
            {
                IsCompleted = true,
            };
        }

        public static LicenseRefreshCacheExpiryResult Failure(string errorKey, string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseRefreshCacheExpiryResult
            {
                IsCompleted = false,
                ErrorKey = errorKey,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}