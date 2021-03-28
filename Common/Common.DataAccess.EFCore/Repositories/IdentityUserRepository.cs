/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using Common.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Common.DataAccess.EFCore
{
    public class IdentityUserRepository : BaseRepository<User, DataContext>, IIdentityUserRepository<User>
    {
        public IdentityUserRepository(DataContext context) : base(context) { }

        public override async Task<bool> Exists(User obj, ContextSession session)
        {
            var context = GetContext(session);

            return await context
                .Set<User>()
                .Where(x => x.Id == obj.Id)
                .AsNoTracking()
                .CountAsync() > 0;
        }

        public override async Task<User> Edit(User obj, ContextSession session)
        {
            var objectExists = await Exists(obj, session);
            var context = GetContext(session);

            context.Entry(obj).State = objectExists ? EntityState.Modified : EntityState.Added;
            await context.SaveChangesAsync();
            return obj;
        }

        public override async Task<User> Get(int id, ContextSession session)
        {
            var context = GetContext(session);

            return await context.Set<User>()
                .Where(obj => obj.Id == id)
                .AsNoTracking()
                .Include(u => u.Claims)
                .Include(u => u.UserRoles)
                    .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetByUserName(string username, ContextSession session)
        {
            var context = GetContext(session);

            return await context.Set<User>()
                .Where(obj => obj.UserName == username)
                .Include(u => u.Claims)
                .Include(u => u.UserRoles)
                    .ThenInclude(x => x.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmail(string email, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                .Include(u => u.UserRoles)
                    .ThenInclude(x => x.Role)
                .Include(u => u.Claims)
                .Where(obj => obj.Email == email)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetById(int id, ContextSession session)
        {
            return await Get(id, session);
        }

        public async Task<IList<User>> GetUsersByRole(int roleId, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                .AsNoTracking()
                .Include(u => u.Claims)
                .Include(u => u.UserRoles)
                    .ThenInclude(x => x.Role)
                .Where(x => x.UserRoles.Any(ur => ur.RoleId == roleId))
                .ToArrayAsync();
        }

        public async Task<IList<User>> GetUsersByClaim(string claimType, string claimValue, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                .AsNoTracking()
                .Include(u => u.Claims)
                .Include(u => u.UserRoles)
                    .ThenInclude(x => x.Role)
                .Where(x => x.Claims.Any(cl => cl.ClaimType == claimType && cl.ClaimValue == claimValue))
                .ToArrayAsync();
        }
    }
}
