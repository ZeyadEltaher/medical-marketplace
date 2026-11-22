using System;
using System.Collections.Generic;
using System.Text;

namespace Medical.BLL.ResultModels.Account
{
    public class ChangePasswordResult
    {
        public bool IsSuccess { get; private set; }
        public string? Key { get; private set; }
        public string? ErrorMessage { get; private set; }

        public static ChangePasswordResult Success()
        {
            return new ChangePasswordResult
            {
                IsSuccess = true,
                Key = string.Empty,
                ErrorMessage = string.Empty,
            };
        }

        public static ChangePasswordResult Failure(string key, string errorMessage)
        {
            return new ChangePasswordResult
            {
                IsSuccess = false,
                Key = key,
                ErrorMessage = errorMessage,
            };
        }
    }
}
