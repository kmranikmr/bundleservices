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
    public class UserRoleRepository : IUserRoleRepository<UserRole>
    {
        protected DataContext dbContext;

        public UserRoleRepository(DataContext context)
        {
            dbContext = context;
        }

        protected DataContext GetContext(ContextSession session)
        {
            dbContext.Session = session;
            return dbContext;
        }

        public async Task<UserRole> Add(UserRole userRole, ContextSession session)
        {
            var context = GetContext(session);
            context.Entry(userRole).State = EntityState.Added;
            await context.SaveChangesAsync();

            await context.Entry(userRole).Reference(ur => ur.Role).LoadAsync();

            return userRole;
        }

        public async Task Delete(int userId, int roleId, ContextSession session)
        {
            var context = GetContext(session);
            var itemToDelete = new UserRole { UserId = userId, RoleId = roleId };
            context.Entry(itemToDelete).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }

        public async Task<UserRole> Get(int userId, int roleId, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<UserRole>().Where(obj => obj.RoleId == roleId && obj.UserId == userId).AsNoTracking().FirstOrDefaultAsync();
        }

        public async Task<IList<string>> GetByUserId(int userId, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<UserRole>()
                .AsNoTracking()
                .Where(obj => obj.UserId == userId)
                .Include(obj => obj.Role)
                .Select(obj => obj.Role.Name)
                .ToListAsync();
        }
    }
}
