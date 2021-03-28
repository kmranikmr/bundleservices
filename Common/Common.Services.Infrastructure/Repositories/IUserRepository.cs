/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using System.Threading.Tasks;

namespace Common.Services.Infrastructure
{
    public interface IUserRepository<TUser> where TUser : User
    {
        Task Delete(int id, ContextSession session);
        Task<TUser> GetByUserName(string username, ContextSession session);
        Task<TUser> GetByEmail(string email, ContextSession session);
        Task<TUser> Get(int id, ContextSession session);
        Task<TUser> Edit(TUser user, ContextSession session);
    }
}
