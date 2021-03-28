/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Common.Services.Infrastructure
{
    public interface IIdentityUserRepository<TUser> where TUser : User
    {
        Task Delete(int id, ContextSession session);
        Task<TUser> GetById(int id, ContextSession session);
        Task<TUser> GetByUserName(string username, ContextSession session);
        Task<IList<TUser>> GetUsersByRole(int roleId, ContextSession session);
        Task<IList<TUser>> GetUsersByClaim(string claimType, string claimValue, ContextSession session);
        Task<TUser> GetByEmail(string email, ContextSession session);
        Task<TUser> Edit(TUser user, ContextSession session);
    }
}
