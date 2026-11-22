using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseDeleteResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }

        public static LicenseDeleteResult Success(string successMessage)
        {
            return new LicenseDeleteResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage,
            };
        }

        public static LicenseDeleteResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseDeleteResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
