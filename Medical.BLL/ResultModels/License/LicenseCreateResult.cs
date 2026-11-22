using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseCreateResult
    {
        public bool IsCompleted { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }
        public string LicenseId { get; private set; } = string.Empty;

        public static LicenseCreateResult Success(string successMessage, string licenseId)
        {
            return new LicenseCreateResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage,
                LicenseId = licenseId,
            };
        }

        public static LicenseCreateResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseCreateResult
            {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }

        public static LicenseCreateResult Failure(string key, string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseCreateResult
            {
                IsCompleted = false,
                Key = key,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}