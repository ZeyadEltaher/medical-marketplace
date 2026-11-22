using Medical.DAL.Identity;

namespace Medical.BLL.ResultModels.Account
{
    public class ExternalLoginResult
    {
        public bool IsSuccess { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ExternalLoginResult Success()
        {
            return new ExternalLoginResult
            {
                IsSuccess = true,
                ErrorMessage = string.Empty,
            };
        }

        public static ExternalLoginResult Failure(string errorMessage)
        {
            return new ExternalLoginResult
            {
                IsSuccess = false,
                ErrorMessage = errorMessage,
            };
        }
    }
}
