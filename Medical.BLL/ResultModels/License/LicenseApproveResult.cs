using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseApproveResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }

        public static LicenseApproveResult Success(string successMessage)
        {
            return new LicenseApproveResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage,
            };
        }

        public static LicenseApproveResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseApproveResult
            {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}