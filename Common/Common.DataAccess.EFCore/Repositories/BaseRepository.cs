/*
* Copyright (c) Akveo 2019. All Rights Reserved.
* Licensed under the Personal / Commercial License.
* See LICENSE_PERSONAL / LICENSE_COMMERCIAL in the project root for license information on type of purchased license.
*/

using Common.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Common.DataAccess.EFCore
{
    public abstract class BaseRepository<TType, TContext>
        where TType : BaseEntity, new()
        where TContext : DataContext
    {
        protected TContext dbContext;

        protected BaseRepository(TContext context)
        {
            this.dbContext = context;
        }

        protected TContext GetContext(ContextSession session)
        {
            dbContext.Session = session;
            return dbContext;
        }

        public abstract Task<TType> Get(int id, ContextSession session);
        public abstract Task<bool> Exists(TType obj, ContextSession session);

        public virtual async Task<TType> Edit(TType obj, ContextSession session)
        {
            var objectExists = await Exists(obj, session);
            var context = GetContext(session);

            context.Entry(obj).State = objectExists ? EntityState.Modified : EntityState.Added;
            await context.SaveChangesAsync();
            return obj;

        }

        public virtual async Task Delete(int id, ContextSession session)
        {
            var context = GetContext(session);

            var itemToDelete = new TType { Id = id };
            context.Entry(itemToDelete).State = EntityState.Deleted;
            await context.SaveChangesAsync();
        }
    }
}
