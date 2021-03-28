/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using System;
using System.Security.Claims;

namespace Common.WebApiCore
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal principal)
        {
            if (principal == null)
                throw new ArgumentNullException(nameof(principal));

            var stringId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            int.TryParse(stringId, out var currentUserId);

            return currentUserId;
        }
    }
}
