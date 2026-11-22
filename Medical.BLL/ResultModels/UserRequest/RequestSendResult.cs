using Medical.BLL.Constants;

namespace Medical.BLL.ResultModels.UserRequest
{
    public class RequestSendResult
    {
        public bool IsCompleted { get; private set; }
        public string? RequestId { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public string SuccessMessage { get; private set; } = string.Empty;
        public RequestFailureReason? FailureReason { get; private set; }

        public static RequestSendResult Success(string successMessage)
        {
            return new RequestSendResult
            {
                IsCompleted = true,
                SuccessMessage = successMessage
            };
        }

        public static RequestSendResult Failure(string errorMessage, RequestFailureReason? failureReason = null, string? requestId = null)
        {
            return new RequestSendResult {
                IsCompleted = false,
                RequestId = requestId,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
