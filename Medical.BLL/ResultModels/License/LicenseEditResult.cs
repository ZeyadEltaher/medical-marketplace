using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseEditResult
    {
        public bool IsCompleted { get; private set; }
        public string Key { get; private set; } = string.Empty;
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }

        public static LicenseEditResult Success(string successMessage)
        {
            return new LicenseEditResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage
            };
        }

        public static LicenseEditResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseEditResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    
        public static LicenseEditResult Failure(string key, string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseEditResult {
                IsCompleted = false,
                Key = key,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
