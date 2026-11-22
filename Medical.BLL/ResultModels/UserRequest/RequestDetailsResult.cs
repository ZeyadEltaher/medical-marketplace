using Medical.BLL.Constants;
using Medical.BLL.ViewModels.UserRequest;

namespace Medical.BLL.ResultModels.UserRequest
{
    public class RequestDetailsResult
    {
        public bool IsCompleted { get; private set; }
        public bool IsSeller { get; private set; }
        public bool IsBuyer { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public RequestFailureReason? FailureReason { get; private set; } 
        public RequestDetailsViewModel Model { get; set; } = null!;

        public static RequestDetailsResult Success(RequestDetailsViewModel model, bool isSeller, bool isBuyer)
        {
            return new RequestDetailsResult
            {
                IsCompleted = true,
                Model = model,
                IsSeller = isSeller,
                IsBuyer = isBuyer,
            };
        }

        public static RequestDetailsResult Failure(string errorMessage, RequestFailureReason? failureReason = null)
        {
            return new RequestDetailsResult {
                IsCompleted = false,
                ErrorMessage = errorMessage,
                FailureReason = failureReason,
            };
        }
    }
}
