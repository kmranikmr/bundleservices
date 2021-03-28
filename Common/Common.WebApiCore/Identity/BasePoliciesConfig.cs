/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Microsoft.AspNetCore.Authorization;

namespace Common.WebApiCore.Identity
{
    public static class BasePoliciesConfig
    {
        public static void RegisterPolicies(this AuthorizationOptions opt)
        {
            opt.AddPolicy("AdminOnly", policy => policy.RequireRole(Roles.Admin));
        }
    }
}
