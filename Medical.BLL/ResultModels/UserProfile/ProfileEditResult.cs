namespace Medical.BLL.ResultModels.UserProfile
{
    public class ProfileEditResult
    {
        public bool IsCompleted { get; private set; }
        public string? Key { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ProfileEditResult Success()
        {
            return new ProfileEditResult
            {
                IsCompleted = true,
                Key = string.Empty,
                ErrorMessage = string.Empty,
            };
        }

        public static ProfileEditResult Failure(string key, string errorMessage)
        {
            return new ProfileEditResult {
                IsCompleted = false,
                Key = key,
                ErrorMessage = errorMessage,
            };
        }
    }
}
