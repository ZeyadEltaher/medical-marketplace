using Medical.BLL.ViewModels.License;

namespace Medical.BLL.ResultModels.License
{
    public class LicenseRestoreResult
    {
        public bool IsCompleted;
        public string ErrorKey { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public LicenseCreateViewModel? Model { get; set; }
        public static LicenseRestoreResult Success(LicenseCreateViewModel? model)
        {
            return new LicenseRestoreResult
            {
                IsCompleted = true,
                Model = model ?? new(),
            };
        }

        public static LicenseRestoreResult Failure(string errorKey, string errorMessage)
        {
            return new LicenseRestoreResult
            {
                IsCompleted = false,
                Model = new(),
                ErrorKey = errorKey,
                ErrorMessage = errorMessage,
            };
        }
    }
}