using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.UserRequest
{
    public class RequestCancelResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public RequestFailureReason? FailureReason { get; private set; }

        public static RequestCancelResult Success(string successMessage)
        {
            return new RequestCancelResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage
            };
        }

        public static RequestCancelResult Failure(string errorMessage, RequestFailureReason? failureReason = null)
        {
            return new RequestCancelResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
