/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DTO;
using Common.WebApiCore.Identity;
using System.Threading.Tasks;

namespace Common.WebApiCore
{
    public interface IAuthenticationService
    {
        Task<AuthResult> Login(LoginDTO loginDto);
        Task<AuthResult> ChangePassword(ChangePasswordDTO changePasswordDto, int currentUserId);
        Task<AuthResult> SignUp(SignUpDTO signUpDto);
        Task<AuthResult> RequestPassword(RequestPasswordDTO requestPasswordDto);
        Task<AuthResult> RestorePassword(RestorePasswordDTO restorePasswordDto);
        Task<AuthResult> SignOut();
    }
}