/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using Common.IdentityManagementCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Common.WebApiCore
{
    public static class IdentityConfig
    {
        public static void Configure(IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 4;
                options.User.RequireUniqueEmail = true;
            });

            services.AddTransient<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddTransient<ILookupNormalizer, UpperInvariantLookupNormalizer>();
            services.AddTransient<IdentityErrorDescriber>();

            services.AddTransient<IRoleStore<Role>, RoleStore<Role>>();
            services.AddTransient<IUserStore<User>, UserStore<User, Role, UserRole, UserClaim>>();
            services.AddTransient<UserManager<User>, ApplicationUserManager>();

            var identityBuilder = new IdentityBuilder(typeof(User), typeof(User), services);
            identityBuilder.AddDefaultTokenProviders();
        }
    }
}
