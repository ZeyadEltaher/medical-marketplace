namespace Medical.BLL.ResultModels.Account
{
    public class LoginResult
    {
        public bool IsSuccess { get; private set; }
        public string? Key { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static LoginResult Success()
        {
            return new LoginResult
            {
                IsSuccess = true,
                Key = string.Empty,
                ErrorMessage = string.Empty,
            };
        }

        public static LoginResult Failure(string key, string errorMessage)
        {
            return new LoginResult {
                IsSuccess = false,
                Key = key,
                ErrorMessage = errorMessage,
            };
        }
    }
}
