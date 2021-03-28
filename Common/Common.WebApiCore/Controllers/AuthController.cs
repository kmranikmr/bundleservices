/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Common.WebApiCore.Controllers
{
    [Route("auth")]
    public class AuthController : BaseApiController
    {
        protected readonly IAuthenticationService authService;

        public AuthController(IAuthenticationService authService)
        {
            this.authService = authService;
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDTO loginDto)
        {
            var result = await authService.Login(loginDto);

            if (result.Succeeded)
            {
                return Ok(new { id= result.UserId, token = result.Token });
            }
            if (result.IsModelValid)
            {
                return Unauthorized();
            }

            return BadRequest();
        }

        [HttpPost]
        [Authorize]
        [Route("reset-pass")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO changePasswordDto)
        {
            var currentUserId = User.GetUserId();

            var result = await authService.ChangePassword(changePasswordDto, currentUserId);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("sign-up")]
        public async Task<IActionResult> SignUp(SignUpDTO signUpDto)
        {
            var result = await authService.SignUp(signUpDto);

            if (result.Succeeded)
                return Ok(new { id = result.UserId, token = result.Token });

            if (result.IsModelValid)
                return Unauthorized();

            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("request-pass")]
        public async Task<IActionResult> RequestPassword(RequestPasswordDTO requestPasswordDto)
        {
            var result = await authService.RequestPassword(requestPasswordDto);

            if (result.Succeeded)
                return Ok(new { result.Token, Description = "Reset Token should be sent via Email. Token in response - just for testing purpose." });

            return BadRequest();
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("restore-pass")]
        public async Task<IActionResult> RestorePassword(RestorePasswordDTO restorePasswordDto)
        {
            var result = await authService.RestorePassword(restorePasswordDto);            

            if (result.Succeeded)
                return Ok(new { result.Token });

            return BadRequest();
        }

        [HttpPost]
        [Authorize]
        [Route("sign-out")]
        public async Task<IActionResult> SignOut()
        {
            return Ok();
        }
    }
}
