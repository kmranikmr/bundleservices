/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DTO;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Common.WebApiCore.Identity
{
    public class AuthenticationService<TUser> : IAuthenticationService
        where TUser : Entities.User, new()
    {
        protected readonly UserManager<TUser> userManager;
        protected readonly JwtManager jwtManager;

        public AuthenticationService(JwtManager jwtManager, UserManager<TUser> userManager)
        {
            this.userManager = userManager;
            this.jwtManager = jwtManager;
        }

        public async Task<AuthResult> Login(LoginDTO loginDto)
        {
            if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
                return AuthResult.UnvalidatedResult(0);

            var user = await userManager.FindByEmailAsync(loginDto.Email);

            if (user != null && user.Id > 0)
            {
                if (await userManager.CheckPasswordAsync(user, loginDto.Password))
                {
                    var token = jwtManager.GenerateToken(user);
                    return AuthResult.TokenResult(user.Id, token);
                }
            }

            return AuthResult.UnauthorizedResult(0);
        }

        public async Task<AuthResult> ChangePassword(ChangePasswordDTO changePasswordDto, int currentUserId)
        {
            if (changePasswordDto == null ||
                string.IsNullOrEmpty(changePasswordDto.ConfirmPassword) ||
                string.IsNullOrEmpty(changePasswordDto.Password) ||
                changePasswordDto.Password != changePasswordDto.ConfirmPassword
            )
                return AuthResult.UnvalidatedResult(0);

            if (currentUserId > 0)
            {
                var user = await userManager.FindByIdAsync(currentUserId.ToString());
                var result = await userManager.ChangePasswordAsync(user, null, changePasswordDto.Password);
                if (result.Succeeded)
                    return AuthResult.SucceededResult(currentUserId);
            }

            return AuthResult.UnauthorizedResult(0);
        }

        public async Task<AuthResult> SignUp(SignUpDTO signUpDto)
        {
            if (signUpDto == null ||
                string.IsNullOrEmpty(signUpDto.Email) ||
                string.IsNullOrEmpty(signUpDto.Password) ||
                string.IsNullOrEmpty(signUpDto.ConfirmPassword) ||
                string.IsNullOrEmpty(signUpDto.FullName) ||
                signUpDto.Password != signUpDto.ConfirmPassword
            )
                return AuthResult.UnvalidatedResult(0);

            var newUser = new TUser { UserName = signUpDto.FullName, Email = signUpDto.Email };

            var result = await userManager.CreateAsync(newUser, signUpDto.Password);

            if (result.Succeeded)
            {
                if (newUser.Id > 0)
                {
                    await userManager.AddToRoleAsync(newUser, "User");
                    var token = jwtManager.GenerateToken(newUser);
                    return AuthResult.TokenResult(newUser.Id, token);
                }
            }

            return AuthResult.UnauthorizedResult(0);
        }

        public async Task<AuthResult> RequestPassword(RequestPasswordDTO requestPasswordDto)
        {
            if (requestPasswordDto == null ||
                string.IsNullOrEmpty(requestPasswordDto.Email))
                return AuthResult.UnvalidatedResult(0);

            var user = await userManager.FindByEmailAsync(requestPasswordDto.Email);

            if (user != null && user.Id > 0)
            {
                var passwordResetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                return AuthResult.TokenResult(user.Id, passwordResetToken);
            }

            return AuthResult.UnvalidatedResult(0);
        }

        public async Task<AuthResult> RestorePassword(RestorePasswordDTO restorePasswordDto)
        {
            if (restorePasswordDto == null ||
                string.IsNullOrEmpty(restorePasswordDto.Email) ||
                string.IsNullOrEmpty(restorePasswordDto.Token) ||
                string.IsNullOrEmpty(restorePasswordDto.NewPassword) ||
                string.IsNullOrEmpty(restorePasswordDto.ConfirmPassword) ||
                string.IsNullOrEmpty(restorePasswordDto.ConfirmPassword) ||
                restorePasswordDto.ConfirmPassword != restorePasswordDto.NewPassword
            )
                return AuthResult.UnvalidatedResult(0);

            var user = await userManager.FindByEmailAsync(restorePasswordDto.Email);

            if (user != null && user.Id > 0)
            {
                var result = await userManager.ResetPasswordAsync(user, restorePasswordDto.Token, restorePasswordDto.NewPassword);

                if (result.Succeeded)
                {
                    var token = jwtManager.GenerateToken(user);
                    return AuthResult.TokenResult(user.Id, token);
                }
            }

            return AuthResult.UnvalidatedResult(0);
        }

        public Task<AuthResult> SignOut()
        {
            throw new System.NotImplementedException();
        }
    }
}
