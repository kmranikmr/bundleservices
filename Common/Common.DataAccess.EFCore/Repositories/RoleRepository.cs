/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using Common.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Common.DataAccess.EFCore
{
    public class RoleRepository : BaseRepository<Role, DataContext>, IRoleRepository<Role>
    {
        public RoleRepository(DataContext context) : base(context) { }

        public override async Task<bool> Exists(Role obj, ContextSession session)
        {
            var context = GetContext(session);

            return await context.Set<Role>().Where(x => x.Id == obj.Id).AsNoTracking().CountAsync() > 0;
        }

        public async Task<Role> Get(string name, ContextSession session)
        {
            var context = GetContext(session);

            return await context.Set<Role>().Where(obj => obj.Name == name).AsNoTracking().FirstOrDefaultAsync();
        }

        public override async Task<Role> Get(int id, ContextSession session)
        {
            var context = GetContext(session);

            return await context.Set<Role>().Where(obj => obj.Id == id).AsNoTracking().FirstOrDefaultAsync();
        }
    }
}
