using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.UserRequest
{
    public class RequestApproveResult
    {
        public bool IsCompleted { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public RequestFailureReason? FailureReason { get; private set; }

        public static RequestApproveResult Success(string successMessage)
        {
            return new RequestApproveResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage
            };
        }

        public static RequestApproveResult Failure(string errorMessage, RequestFailureReason? failureReason = null)
        {
            return new RequestApproveResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
