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
    public class UserRepository : BaseRepository<User, DataContext>, IUserRepository<User>
    {
        public UserRepository(DataContext context) : base(context) { }

        public override async Task<bool> Exists(User obj, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>().Where(x => x.Id == obj.Id).AsNoTracking().CountAsync() > 0;
        }

        public override async Task<User> Edit(User obj, ContextSession session)
        {
            var objectExists = await Exists(obj, session);
            var context = GetContext(session);
            context.Entry(obj).State = objectExists ? EntityState.Modified : EntityState.Added;

            if (string.IsNullOrEmpty(obj.Password))
            {
                context.Entry(obj).Property(x => x.Password).IsModified = false;
            }
            await context.SaveChangesAsync();
            return obj;
        }

        public override async Task<User> Get(int id, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                   .Where(obj => obj.Id == id)
                   .AsNoTracking()
                   .Include(u => u.UserRoles)
                       .ThenInclude(x => x.Role)
                   .FirstOrDefaultAsync();
        }

        public async Task<User> GetByUserName(string username, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                    .Where(obj => obj.UserName == username)
                    .Include(u => u.UserRoles)
                        .ThenInclude(x => x.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmail(string email, ContextSession session)
        {
            var context = GetContext(session);
            return await context.Set<User>()
                    .Where(obj => obj.Email == email)
                    .Include(u => u.UserRoles)
                        .ThenInclude(x => x.Role)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
        }
    }
}
