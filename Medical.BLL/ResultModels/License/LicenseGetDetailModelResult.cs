using Medical.BLL.Constants;
using Medical.BLL.ViewModels.License;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseGetDetailsModelResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public LicenseFailureReason? FailureReason { get; private set; }
        public LicenseDetailsViewModel Model { get; private set; } = null!;

        public static LicenseGetDetailsModelResult Success (LicenseDetailsViewModel model)
        {
            return new LicenseGetDetailsModelResult
            {
                IsCompleted = true,
                Model = model,
            };
        }

        public static LicenseGetDetailsModelResult Failure(string errorMessage, LicenseFailureReason? failureReason = null)
        {
            return new LicenseGetDetailsModelResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
