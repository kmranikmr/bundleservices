/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

namespace Common.WebApiCore.Identity
{
    public class AuthResult
    {
        private AuthResult(int userId, string token)
        {
            Succeeded = true;
            IsModelValid = true;
            Token = token;
            UserId = userId;
        }

        private AuthResult(int userId, bool isSucceeded, bool isModelValid)
        {
            Succeeded = isSucceeded;
            IsModelValid = isModelValid;
            UserId = userId;
        }

        private AuthResult(int userId, bool isSucceeded)
        {
            Succeeded = isSucceeded;
            IsModelValid = isSucceeded;
            UserId = userId;
        }

        public bool Succeeded { get; }
        public string Token { get; }
        public bool IsModelValid { get; }
        public int UserId { get; set; }

        public static AuthResult UnvalidatedResult(int userId) { return new AuthResult(userId, false); }
        public static AuthResult UnauthorizedResult(int userId) { return new AuthResult(userId, false, true); }
        public static AuthResult SucceededResult(int userId) { return new AuthResult(userId, true); }
        public static AuthResult TokenResult(int userId, string token)
        {
            return new AuthResult(userId, token);
        }
    }
}