using Medical.BLL.Constants;
using Medical.BLL.ViewModels.License;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseGetEditModelResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }
        public LicenseEditViewModel Model { get; private set; } = null!;

        public static LicenseGetEditModelResult Success (LicenseEditViewModel model)
        {
            return new LicenseGetEditModelResult
            {
                IsCompleted = true,
                Model = model,
            };
        }

        public static LicenseGetEditModelResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseGetEditModelResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
