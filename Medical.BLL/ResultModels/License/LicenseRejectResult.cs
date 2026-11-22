using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseRejectResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }

        public static LicenseRejectResult Success(string successMessage)
        {
            return new LicenseRejectResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage,
            };
        }

        public static LicenseRejectResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseRejectResult
            {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason
            };
        }
    }
}