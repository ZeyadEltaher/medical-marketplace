using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.UserRequest
{
    public class RequestRejectResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public RequestFailureReason? FailureReason { get; private set; }

        public static RequestRejectResult Success(string successMessage)
        {
            return new RequestRejectResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage
            };
        }

        public static RequestRejectResult Failure(string errorMessage, RequestFailureReason? failureReason = null)
        {
            return new RequestRejectResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
