using Medical.BLL.Constants;
using Medical.BLL.ViewModels.AdminDashboard;

namespace Medical.BLL.ResultModels.AdminDashboardd
{
    public class UserGetDetailsModelResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public AdminFailureReason? FailureReason { get; private set; }
        public UserDetailsViewModel Model { get; private set; } = null!;

        public static UserGetDetailsModelResult Success (UserDetailsViewModel model)
        {
            return new UserGetDetailsModelResult
            {
                IsCompleted = true,
                Model = model,
            };
        }

        public static UserGetDetailsModelResult Failure(string errorMessage, AdminFailureReason? failureReason = null)
        {
            return new UserGetDetailsModelResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
