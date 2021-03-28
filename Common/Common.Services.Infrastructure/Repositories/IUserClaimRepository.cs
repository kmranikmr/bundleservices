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
    public interface IUserClaimRepository<TUserClaim> where TUserClaim : UserClaim
    {
        Task<IList<TUserClaim>> GetByUserId(int userId, ContextSession session);
        Task Delete(int userId, string claimType, string claimValue, ContextSession session);
        Task<TUserClaim> Add(TUserClaim userClaim, ContextSession session);
        Task<IList<TUserClaim>> EditMany(IList<TUserClaim> userClaims, ContextSession session);
        Task<IList<TUserClaim>> GetList(int userId, string claimType, string claimValue, ContextSession session);
    }
}
