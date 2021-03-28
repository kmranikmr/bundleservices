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
    public interface IUserRoleRepository<TUserRole> where TUserRole : UserRole
    {
        Task<TUserRole> Add(TUserRole userRole, ContextSession session);
        Task<TUserRole> Get(int userId, int roleId, ContextSession session);
        Task Delete(int userId, int roleId, ContextSession session);
        Task<IList<string>> GetByUserId(int userId, ContextSession session);
    }
}
