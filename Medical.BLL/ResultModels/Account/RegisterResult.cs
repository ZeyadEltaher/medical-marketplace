using System;
using System.Collections.Generic;
using System.Text;

namespace Medical.BLL.ResultModels.Account
{
    public class RegisterResult
    {
        public string UserId { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public bool IsSuccess { get; private set; }
        public Dictionary<string, List<string>>? Errors { get; private set; }

        public static RegisterResult Success(string userId)
        {
            return new RegisterResult
            {
                UserId = userId,
                IsSuccess = true,
                Errors = null,
            };
        }

        public static RegisterResult Success(string userId, string token)
        {
            return new RegisterResult
            {
                UserId = userId,
                Token = token,
                IsSuccess = true,
                Errors = null,
            };
        }

        public static RegisterResult Failure(Dictionary<string, List<string>> errors)
        {
            return new RegisterResult
            {
                IsSuccess = false,
                Errors = errors,
            };
        }
    }
}
